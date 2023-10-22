using Azure.Core;
using CloseConnectv1.Data;
using CloseConnectv1.Filters;
using CloseConnectv1.Hubs;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Repository.IRepository;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _dbPost;
        private readonly ApplicationDbContext _context;
        private readonly DTOConversion _conversion;
        private readonly PostPopularityCalculator _postPopularity;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public PostController(IPostRepository dbPost, ApplicationDbContext context,
            DTOConversion conversion, PostPopularityCalculator postPopularity,
            UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> notificationHubContext)
        {
            _dbPost = dbPost;
            _context = context;
            _conversion = conversion;
            _postPopularity = postPopularity;
            _userManager = userManager;
            _notificationHubContext = notificationHubContext;
        }

        [HttpGet("GetPost/{postId:int}")]
        public async Task<IActionResult> GetPost(int postId, [FromQuery] string userId)
        {
            try
            {
                if (postId <= 0) return BadRequest();

                Post post = await _dbPost.GetAsync(p => p.PostId == postId, false);

                if (post is null) return NotFound();

                var postDTO = await _conversion.ConvertToPostDisplayDTO(post, userId);

                return Ok(postDTO);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserPosts")]
        public async Task<IActionResult> GetUserPosts([FromQuery] string loggedInUserId, [FromQuery] string userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                ApplicationUser? currentUser = await _userManager.FindByIdAsync(userId);

                if(currentUser is null) return NotFound();  

                List<Post> originalPosts = await _dbPost.GetAllAsync(post => post.AuthorId.Equals(userId), pageNumber, pageSize);

                var finalPosts = originalPosts.Select(async post =>
                {
                    return await _conversion.ConvertToPostDisplayDTO(post, loggedInUserId);
                })
                .Select(t => t.Result)
                .OrderByDescending(t => t.PostId)
                .ToList();

                return Ok(finalPosts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("GetPosts/{loginId}")]
        public async Task<IActionResult> GetPosts(string loginId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Check if user has provided email or username for login
                var isValidEmail = StaticHelpers.CheckIfEmail(loginId);

                ApplicationUser? existingUser;

                // Check if the user exists
                if (isValidEmail) existingUser = await _userManager.FindByEmailAsync(loginId);
                else existingUser = await _userManager.FindByNameAsync(loginId);

                if (existingUser is null) return NotFound();

                List<string> friendIds = new();
                friendIds = await _context.FriendRequests
                    .Where(f => (f.SenderId == existingUser.Id || f.ReceiverId == existingUser.Id) && f.IsAccepted == true)
                    .Select(e => e.SenderId == existingUser.Id ? e.ReceiverId : e.SenderId)
                    .Distinct()
                    .ToListAsync();

                List<Post> friendPosts = await _dbPost.GetAllAsync(post => friendIds.Contains(post.AuthorId), pageNumber, pageSize);

                List<Post> recentPosts = await _dbPost.GetRecentAsync(pageNumber, pageSize);

                List<Post> popularPosts = await _dbPost.GetPopularAsync(pageNumber, pageSize);

                List<Post> mixedPosts = recentPosts.Union(popularPosts).Union(friendPosts).ToList();

                // Shuffle posts
                mixedPosts.Shuffle();

                var finalPosts = mixedPosts
                            .Select(async post =>
                            {
                                return await _conversion.ConvertToPostDisplayDTO(post, existingUser.Id);
                            })
                            .Select(t => t.Result)
                            .ToList();

                return Ok(finalPosts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateDTO createDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (createDTO == null) return BadRequest();

                Post model = _conversion.ConvertPostCreateDTOToPost(createDTO);

                await _dbPost.CreateAsync(model);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdatePost/{postId:int}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromQuery] string userId, [FromQuery] string likesOrComments)
        {
            try
            {
                if (postId <= 0 || likesOrComments is null) return BadRequest();

                Post existingPost = await _dbPost.GetAsync(d => d.PostId == postId, false);

                if (existingPost is null) return NotFound();

                int maxLikes = await _dbPost.GetMaxLikes();
                int maxComments = await _dbPost.GetMaxComments();

                // create notification object
                Notification notif = new()
                {
                    SenderId = userId,
                    RecipientId = existingPost.AuthorId,
                    Timestamp = DateTime.UtcNow,
                    ActionUrl = postId.ToString(),
                };

                // find logged in user details
                ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);

                if (currentUser is null) return BadRequest();

                if (likesOrComments.Equals("likes"))
                {
                    existingPost.NumberOfLikes += 1;
                    bool alreadyLiked = await _context.PostLikes.AnyAsync(post => post.UserId.Equals(userId) && post.PostId == postId);

                    if (alreadyLiked)
                    {
                        return BadRequest("User has already liked this post");
                    }
                    else
                    {
                        var postLike = new PostLike
                        {
                            UserId = userId,
                            PostId = postId
                        };

                        _context.PostLikes.Add(postLike);
                        await _context.SaveChangesAsync();

                        // update notification
                        notif.Message = $"{currentUser.Name} liked your post.";
                    }
                }
                if (likesOrComments.Equals("comments"))
                {
                    existingPost.NumberOfComments += 1;

                    // update notification
                    notif.Message = $"{currentUser.Name} commented on your post.";
                }

                // save notif to DB
                await _context.Notifications.AddAsync(notif);
                await _context.SaveChangesAsync();

                existingPost.PopularityScore = _postPopularity.CalculatePopularity
       (existingPost.NumberOfLikes, existingPost.NumberOfComments, maxLikes, maxComments, existingPost.CreateDate);
                existingPost.UpdateDate = DateTime.UtcNow;

                await _dbPost.UpdateAsync(existingPost);

                NotificationDTO notificationDTO = _conversion.ConvertToNotificationDTO(notif, currentUser);

                // Broadcast the notification to the sender
                try
                {
                    if (!existingPost.AuthorId.Equals(userId))
                    {
                        await _notificationHubContext.Clients.Client(NotificationHub._clientConnections[existingPost.AuthorId]).SendAsync("ReceiveNotification", notificationDTO);
                    }
                }
                catch (Exception ex)
                {
                    // user is not online but handle issue gracefully
                    if (ex.Message.Equals($"The given key '{existingPost.AuthorId}' was not present in the dictionary."))
                    {
                        return Ok();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

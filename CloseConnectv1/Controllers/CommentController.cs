using CloseConnectv1.Data;
using CloseConnectv1.Filters;
using CloseConnectv1.Hubs;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Repository.IRepository;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _dbComment;
        private readonly ApplicationDbContext _context;
        private readonly DTOConversion _conversion;
        private readonly PostPopularityCalculator _postPopularity;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public CommentController(ICommentRepository dbComment, ApplicationDbContext context,
                DTOConversion conversion, PostPopularityCalculator postPopularity,
                UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> notificationHubContext)
        {
            _dbComment = dbComment;
            _context = context;
            _conversion = conversion;
            _postPopularity = postPopularity;
            _userManager = userManager;
            _notificationHubContext = notificationHubContext;
        }

        [HttpPost("CreateComment")]
        public async Task<IActionResult> CreateComment([FromBody] CommentCreateDTO createDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (createDTO == null) return BadRequest();

                Comment model = _conversion.ConvertCommentCreateDTOToComment(createDTO);

                await _dbComment.CreateAsync(model);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllComments/{postId:int}")]
        public async Task<IActionResult> GetAllCommentsForPost([FromQuery] string userId, int postId)
        {
            try
            {
                if (postId <= 0) return BadRequest();

                List<Comment> parentComments = await _dbComment.GetAllAsync(comment => comment.PostId == postId && comment.ParentCommentId == null);

                parentComments = parentComments.OrderBy(comment => comment.CreateDate).ToList();

                var flattenedComments = new List<Comment>();

                foreach (var parentComment in parentComments)
                {
                    flattenedComments.Add(parentComment);

                    List<Comment> postComments = await _dbComment.GetAllAsync(comment => comment.PostId == postId);

                    foreach (var postComment in postComments)
                    {
                        int? ancestorCommentId = StaticHelpers.GetAncestorCommentId(postComments, postComment.CommentId);

                        if(ancestorCommentId is not null && ancestorCommentId == parentComment.CommentId && postComment.CommentId != ancestorCommentId)
                        {
                            flattenedComments.Add(postComment);
                        } 
                    }
                }

                List<CommentDisplayDTO> comments = flattenedComments
                    .Select(async comment =>
                    {
                        return await _conversion.ConvertToCommentDisplayDTO(comment, userId);
                    })
                    .Select(t => t.Result)
                    .ToList();

                comments.ForEach(comment =>
                {
                    comment.CreateDate = comment.CreateDate.ToLocalTime();
                });

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateComment/{commentId:int}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromQuery] string currentUserId, [FromQuery] string likesOrComments)
        {
            try
            {
                if (commentId <= 0 || likesOrComments is null) return BadRequest();

                Comment comment = await _dbComment.GetAsync(comment => comment.CommentId == commentId, false);

                if (comment is null) return NotFound();

                // create notification object
                Notification notif = new()
                {
                    SenderId = currentUserId,
                    RecipientId = comment.AuthorId,
                    Timestamp = DateTime.UtcNow,
                    ActionUrl = comment.PostId.ToString()
                };

                // find logged in user details
                ApplicationUser currentUser = await _userManager.FindByIdAsync(currentUserId);

                if (currentUser is null) return BadRequest();

                if (likesOrComments.Equals("likes"))
                {
                    comment.NumberOfLikes += 1;
                    bool alreadyLiked = await _dbComment.IsCommentLikedAlready(commentId, currentUserId);
                    if (alreadyLiked)
                    {
                        return BadRequest("User has already liked this comment");
                    }
                    else
                    {
                        var commentLike = new CommentLike
                        {
                            UserId = currentUserId,
                            CommentId = commentId
                        };

                        _context.CommentLikes.Add(commentLike);
                        await _context.SaveChangesAsync();

                        // update notification
                        notif.Message = $"{currentUser.Name} liked your comment.";
                    }
                }

                if (likesOrComments.Equals("comments"))
                {
                    comment.NumberOfComments += 1;

                    // update notification
                    notif.Message = $"{currentUser.Name} replied to your comment.";
                }

                // save notif to DB
                await _context.Notifications.AddAsync(notif);
                await _context.SaveChangesAsync();

                await _dbComment.UpdateAsync(comment);

                NotificationDTO notificationDTO = _conversion.ConvertToNotificationDTO(notif, currentUser);

                // Broadcast the notification to the sender
                try
                {
                    if (!comment.AuthorId.Equals(currentUserId))
                    {
                        await _notificationHubContext.Clients.Client(NotificationHub._clientConnections[comment.AuthorId]).SendAsync("ReceiveNotification", notificationDTO);
                    }
                }
                catch (Exception ex)
                {
                    // user is not online but handle issue gracefully
                    if (ex.Message.Equals($"The given key '{comment.AuthorId}' was not present in the dictionary."))
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

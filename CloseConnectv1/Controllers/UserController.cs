using Azure.Identity;
using CloseConnectv1.Data;
using CloseConnectv1.Filters;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static CloseConnectv1.Utilities.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CloseConnectv1.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly DTOConversion _conversion;

        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, DTOConversion conversion)
        {
            _userManager = userManager;
            _context = context;
            _conversion = conversion;
        }

        [HttpGet("Search/{loginId}/{query}")]
        public async Task<IActionResult> SearchUsers(string loginId, string query)
        {
            try
            {
                // Check if user has provided email or username for login
                var isValidEmail = StaticHelpers.CheckIfEmail(loginId);

                ApplicationUser? existingUser;

                // Check if the user exists
                if (isValidEmail)
                {
                    existingUser = await _userManager.FindByEmailAsync(loginId);
                }
                else
                {
                    existingUser = await _userManager.FindByNameAsync(loginId);
                }

                if (existingUser is not null)
                {
                    // Perform the search in the database using EF
                    List<ApplicationUser> results = await _userManager.Users
                        .Where(result => result.Name.Contains(query) || result.UserName.Contains(query))
                        .ToListAsync();

                    List<FriendRequest> sentFriendRequests = await _context.FriendRequests
                                .Where(friendReq => (friendReq.SenderId == existingUser.Id &&
                                (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();

                    var sentFriendRequestIds = sentFriendRequests.Select(f => f.ReceiverId).ToList();

                    List<FriendRequest> receivedFriendRequests = await _context.FriendRequests
                                .Where(friendReq => (friendReq.ReceiverId == existingUser.Id &&
                                (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();

                    var receivedFriendRequestIds = receivedFriendRequests.Select(f => f.SenderId).ToList();

                    var friendIds = await _context.FriendRequests
                        .Where(f => (f.SenderId == existingUser.Id || f.ReceiverId == existingUser.Id) && f.IsAccepted == true)
                        .Select(e => e.SenderId == existingUser.Id ? e.ReceiverId : e.SenderId)
                        .Distinct()
                        .ToListAsync();

                    var searchUserResponse = results.Select(user =>
                    {
                        SearchUserDTO userWithStatus = new();

                        int relationshipCode;

                        if (friendIds.Contains(user.Id)) relationshipCode = 0;
                        else if (sentFriendRequestIds.Contains(user.Id)) relationshipCode = 1;
                        else if (receivedFriendRequestIds.Contains(user.Id)) relationshipCode = 2;
                        else relationshipCode = 3;

                        userWithStatus = _conversion.ConvertToSearchUserDTOAsync(user, relationshipCode);

                        return userWithStatus;
                    }).ToList();

                    return Ok(searchUserResponse);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("UserProfile/{loginId}")]
        public async Task<IActionResult> FetchUserProfile(string loginId)
        {
            try
            {
                // Check if user has provided email or username for login
                var isValidEmail = StaticHelpers.CheckIfEmail(loginId);

                ApplicationUser? existingUser;

                // Check if the user exists
                if (isValidEmail)
                {
                    existingUser = await _userManager.FindByEmailAsync(loginId);
                }
                else
                {
                    existingUser = await _userManager.FindByNameAsync(loginId);
                }

                if (existingUser is not null)
                {
                    UserProfileDTO userProfile = new()
                    {
                        Id = existingUser.Id,
                        Name = existingUser.Name,
                        UserName = existingUser.UserName,
                        DOB = existingUser.DOB,
                        CreateDate = existingUser.CreateDate.ToLocalTime(),
                        DisplayPictureURL = existingUser.DisplayPictureURL,
                        BackgroundPictureURL = existingUser.BackgroundPictureURL,
                    };

                    return Ok(userProfile);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("UserData/{userId}/{loggedInUserId}")]
        public async Task<IActionResult> FetchUserData(string userId, string loggedInUserId)
        {
            try
            {
                ApplicationUser? existingUser = await _userManager.FindByIdAsync(userId);

                ApplicationUser? loggedInUser = await _userManager.FindByIdAsync(loggedInUserId);

                if (existingUser is null || loggedInUser is null) return NotFound();


                // Perform the search in the database using EF

                List<FriendRequest> sentFriendRequests = await _context.FriendRequests
                            .Where(friendReq => (friendReq.SenderId == loggedInUserId &&
                            (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();

                var sentFriendRequestIds = sentFriendRequests.Select(f => f.ReceiverId).ToList();

                List<FriendRequest> receivedFriendRequests = await _context.FriendRequests
                            .Where(friendReq => (friendReq.ReceiverId == loggedInUserId &&
                            (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();

                var receivedFriendRequestIds = receivedFriendRequests.Select(f => f.SenderId).ToList();

                var friendIds = await _context.FriendRequests
                    .Where(f => (f.SenderId == loggedInUserId || f.ReceiverId == loggedInUserId) && f.IsAccepted == true)
                    .Select(e => e.SenderId == loggedInUserId ? e.ReceiverId : e.SenderId)
                    .Distinct()
                    .ToListAsync();

                int relationshipCode;

                if (friendIds.Contains(userId)) relationshipCode = 0;
                else if (sentFriendRequestIds.Contains(userId)) relationshipCode = 1;
                else if (receivedFriendRequestIds.Contains(userId)) relationshipCode = 2;
                else relationshipCode = 3;

                if (existingUser is not null)
                {
                    UserProfileDTO userProfile = new()
                    {
                        Id = existingUser.Id,
                        Name = existingUser.Name,
                        UserName = existingUser.UserName,
                        DOB = existingUser.DOB,
                        CreateDate = existingUser.CreateDate.ToLocalTime(),
                        DisplayPictureURL = existingUser.DisplayPictureURL,
                        BackgroundPictureURL= existingUser.BackgroundPictureURL,
                        RelationshipWithLoggedInUser = (SearchUserRelationshipCodes)relationshipCode
                    };

                    return Ok(userProfile);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateUserPicture/{userId}")]
        public async Task<IActionResult> UpdateUserPhoto(string userId, [FromQuery] string URL, [FromQuery] string photoType)
        {
            try
            {
                if (URL.IsNullOrEmpty() || photoType.IsNullOrEmpty()) return BadRequest();

                ApplicationUser? existingUser = await _userManager.FindByIdAsync(userId);

                if (existingUser is null) return BadRequest();

                switch (photoType)
                {
                    case "displayPicture":
                        existingUser.DisplayPictureURL = URL;
                        break;
                    case "backgroundPicture":
                        existingUser.BackgroundPictureURL = URL; // update this field
                        break;
                    default: break;
                }

                await _userManager.UpdateAsync(existingUser);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

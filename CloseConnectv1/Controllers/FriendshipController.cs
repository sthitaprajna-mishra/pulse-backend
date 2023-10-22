using Azure.Core;
using CloseConnectv1.Data;
using CloseConnectv1.Filters;
using CloseConnectv1.Hubs;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection;


namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class FriendshipController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly DTOConversion _conversion;

        public FriendshipController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IHubContext<NotificationHub> notificationHubContext,
            DTOConversion conversion
            )
        {
            _userManager = userManager;
            _context = context;
            _notificationHubContext = notificationHubContext;
            _conversion = conversion;
        }

        [HttpPost("SendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDTO requestDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var sender = await _userManager.FindByIdAsync(requestDTO.SenderId);
                var receiver = await _userManager.FindByIdAsync(requestDTO.ReceiverId);

                if (sender is not null && receiver is not null)
                {
                    //sender.SentFriendRequests ??= new List<FriendRequest> { };
                    //receiver.ReceivedFriendRequests ??= new List<FriendRequest> { };

                    var newRequest = new FriendRequest
                    {
                        SenderId = requestDTO.SenderId,
                        ReceiverId = requestDTO.ReceiverId,
                        IsAccepted = false,
                        IsDeclined = false,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _context.FriendRequests.AddAsync(newRequest);
                    await _context.SaveChangesAsync();

                    FriendRequestResponseDTO friendRequestDTO = await _conversion.ConvertToFriendRequestResponseDTOAsync(newRequest);

                    // Broadcast the notification to the sender
                    try
                    {
                        await _notificationHubContext.Clients.Client(NotificationHub._clientConnections[newRequest.ReceiverId]).SendAsync("ReceiveNotification", friendRequestDTO);
                    }
                    catch (Exception ex)
                    {
                        // user is not online but handle issue gracefully
                        if (ex.Message.Equals($"The given key '{newRequest.ReceiverId}' was not present in the dictionary."))
                        {
                            return Ok();
                        }
                    }

                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AcceptOrDecline")]
        public async Task<IActionResult> AcceptOrDeclineFriendRequest([FromBody] AcceptOrDeclineFriendRequestDTO requestDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                FriendRequest friendRequest = await _context.FriendRequests.FirstOrDefaultAsync(f => f.FriendRequestId == requestDTO.FriendRequestId);

                if (friendRequest is null)
                {
                    return NotFound(); // Friend request not found
                }

                // create notification object
                Notification notif = new()
                {
                    SenderId = friendRequest.ReceiverId,
                    RecipientId = friendRequest.SenderId,
                    Timestamp = DateTime.UtcNow
                    // ActionUrl
                };

                if (requestDTO.IsAccepted)
                {
                    // Add the number of friends of both users
                    var sender = await _userManager.FindByIdAsync(friendRequest.SenderId);
                    var receiver = await _userManager.FindByIdAsync(friendRequest.ReceiverId);

                    if (sender is not null && receiver is not null)
                    {
                        sender.NumberOfFriends++;
                        receiver.NumberOfFriends++;

                        friendRequest.IsAccepted = true;
                        await _context.SaveChangesAsync();

                        // update notification
                        notif.Message = $"{sender.Name} has accepted your friend request.";

                        // save notif to DB
                        await _context.Notifications.AddAsync(notif);
                        await _context.SaveChangesAsync();

                        // Broadcast the notification to the sender
                        try
                        {
                            await _notificationHubContext.Clients.Client(NotificationHub._clientConnections[friendRequest.SenderId]).SendAsync("ReceiveNotification", notif);
                        }
                        catch (Exception ex)
                        {
                            // user is not online but handle issue gracefully
                            if (ex.Message.Equals($"The given key '{friendRequest.SenderId}' was not present in the dictionary."))
                            {
                                return Ok();
                            }
                        }


                        return Ok(); // Friend request accepted successfully
                    }

                    return NotFound();
                }

                if (requestDTO.IsDeclined)
                {
                    _context.FriendRequests.Remove(friendRequest);
                    await _context.SaveChangesAsync();

                    var sender = await _userManager.FindByIdAsync(friendRequest.SenderId);
                    var receiver = await _userManager.FindByIdAsync(friendRequest.ReceiverId);

                    // update notification
                    notif.Message = $"{receiver.Name} has declined your friend request.";

                    // save notif to DB
                    await _context.Notifications.AddAsync(notif);
                    await _context.SaveChangesAsync();

                    // Broadcast the notification to the sender
                    try
                    {
                        await _notificationHubContext.Clients.Client(NotificationHub._clientConnections[friendRequest.SenderId]).SendAsync("ReceiveNotification", notif);
                    }
                    catch (Exception ex)
                    {
                        // user is not online but handle issue gracefully
                        if (ex.Message.Equals($"The given key '{friendRequest.SenderId}' was not present in the dictionary."))
                        {
                            return Ok();
                        }
                    }

                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllFriendRequests/{loginId}/{requestType}")] // requestType - sent / received
        public async Task<IActionResult> GetAllFriendRequests(string loginId, string requestType)
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

                if (existingUser is null)
                {
                    return BadRequest();
                }
                List<FriendRequest> requests = new();

                switch (requestType)
                {
                    case "sent":
                        requests = await _context.FriendRequests
                            .Where(friendReq => (friendReq.SenderId == existingUser.Id &&
                            (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();
                        break;
                    case "received":
                        requests = await _context.FriendRequests
                            .Where(friendReq => (friendReq.ReceiverId == existingUser.Id &&
                            (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();
                        break;
                    default:
                        break;
                }

                List<FriendRequestResponseDTO> friendRequestDTOs = requests
                                            .Select(async friendRequest =>
                                            {
                                                return await _conversion.ConvertToFriendRequestResponseDTOAsync(friendRequest);
                                            })
                                            .Select(t => t.Result)
                                            .OrderByDescending(t => t.FriendRequestId)
                                            .ToList();

                return Ok(friendRequestDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("GetNumberOfFriendRequests/{loginId}/{requestType}")] // requestType - sent / received
        //public async Task<IActionResult> GetNumberOfFriendRequests(string loginId, string requestType)
        //{
        //    try
        //    {
        //        string token = HttpContext.Request.Headers["Authorization"].ToString();

        //        if (token.StartsWith("Bearer "))
        //        {
        //            token = token[7..];
        //            if (_tokenValidator.IsTokenExpired(token))
        //            {
        //                return Forbid();
        //            }
        //            else
        //            {
        //                // Check if user has provided email or username for login
        //                var isValidEmail = StaticHelpers.CheckIfEmail(loginId);

        //                ApplicationUser? existingUser;

        //                // Check if the user exists
        //                if (isValidEmail)
        //                {
        //                    existingUser = await _userManager.FindByEmailAsync(loginId);
        //                }
        //                else
        //                {
        //                    existingUser = await _userManager.FindByNameAsync(loginId);
        //                }

        //                if (existingUser is not null)
        //                {
        //                    int numberOfRequests = 0;

        //                    switch (requestType)
        //                    {
        //                        case "sent":
        //                            numberOfRequests = await _context.FriendRequests
        //                                .Where(friendReq => (friendReq.SenderId == existingUser.Id &&
        //                                (friendReq.IsAccepted == false && friendReq.IsDeclined == false)))
        //                                .Distinct().CountAsync();
        //                            break;
        //                        case "received":
        //                            numberOfRequests = await _context.FriendRequests
        //                                .Where(friendReq => (friendReq.ReceiverId == existingUser.Id &&
        //                                (friendReq.IsAccepted == false && friendReq.IsDeclined == false)))
        //                                .Distinct().CountAsync();
        //                            break;
        //                        default:
        //                            break;
        //                    }
        //                    return Ok(numberOfRequests);
        //                }
        //                else
        //                {
        //                    return BadRequest();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("GetAllFriends/{loginId}")]
        public async Task<IActionResult> GetAllFriends(string loginId)
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
                    List<string> requests = new();
                    requests = await _context.FriendRequests
                        .Where(f => (f.SenderId == existingUser.Id || f.ReceiverId == existingUser.Id) && f.IsAccepted == true)
                        .Select(e => e.SenderId == existingUser.Id ? e.ReceiverId : e.SenderId)
                        .Distinct()
                        .ToListAsync();
                    return Ok(requests);
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

        [HttpGet("GetAllFriendsByUserId/{userId}/{loggedInUserId}")]
        public async Task<IActionResult> GetAllFriendsByUserId(string userId, string loggedInUserId)
        {
            try
            {
                ApplicationUser? existingUser = await _userManager.FindByIdAsync(userId);
                ApplicationUser? loggedInUser = await _userManager.FindByIdAsync(loggedInUserId);


                if (existingUser is not null && loggedInUser is not null)
                {
                    // friends of the user profile being viewed
                    List<string> friendIds = new();
                    friendIds = await _context.FriendRequests
                        .Where(f => (f.SenderId == existingUser.Id || f.ReceiverId == existingUser.Id) && f.IsAccepted == true)
                        .Select(e => e.SenderId == existingUser.Id ? e.ReceiverId : e.SenderId)
                        .Distinct()
                        .ToListAsync();

                    // friend requests sent by logged in user
                    List<FriendRequest> sentFriendRequests = await _context.FriendRequests
            .Where(friendReq => (friendReq.SenderId == loggedInUserId &&
            (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();

                    var sentFriendRequestIds = sentFriendRequests.Select(f => f.ReceiverId).ToList();

                    // friend requests received by logged in user
                    List<FriendRequest> receivedFriendRequests = await _context.FriendRequests
                                .Where(friendReq => (friendReq.ReceiverId == loggedInUserId &&
                                (friendReq.IsAccepted == false && friendReq.IsDeclined == false))).ToListAsync();

                    var receivedFriendRequestIds = receivedFriendRequests.Select(f => f.SenderId).ToList();

                    // friends of logged in user
                    var loggedInUserFriendIds = await _context.FriendRequests
                        .Where(f => (f.SenderId == loggedInUserId || f.ReceiverId == loggedInUserId) && f.IsAccepted == true)
                        .Select(e => e.SenderId == loggedInUserId ? e.ReceiverId : e.SenderId)
                        .Distinct()
                        .ToListAsync();

                    List<FriendDTO> searchUserResponse = friendIds.Select(async friendId =>
                    {
                        int relationshipCode;

                        if (loggedInUserFriendIds.Contains(friendId)) relationshipCode = 0;
                        else if (sentFriendRequestIds.Contains(friendId)) relationshipCode = 1;
                        else if (receivedFriendRequestIds.Contains(friendId)) relationshipCode = 2;
                        else relationshipCode = 3;

                        return await _conversion.ConvertToFriendDTOAsync(friendId, relationshipCode);
                    })
                        .Select(t => t.Result)
                        .OrderByDescending(t => t.FriendId)
                        .ToList();

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

    }
}

using Azure.Core;
using CloseConnectv1.Data;
using CloseConnectv1.Filters;
using CloseConnectv1.Hubs;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class NotificationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly DTOConversion _conversion;

        public NotificationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, DTOConversion conversion)
        {
            _userManager = userManager;
            _context = context;
            _conversion = conversion;
        }

        [HttpGet("GetAllNotifications/{loginId}")]
        public async Task<IActionResult> GetAllNotifications(string loginId)
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
                    List<Notification> notifications = _context.Notifications
                        .Where(n => n.RecipientId == existingUser.Id && n.SenderId != existingUser.Id)
                        .OrderByDescending(n => n.Id)
                    .ToList();

                    List<NotificationDTO> notifDTOs = notifications
                            .Select(async notif =>
                            {
                                return await _conversion.ConvertToNotificationDTOList(notif, notif.SenderId);
                            })
                            .Select(t => t.Result)
                            .OrderByDescending(t => t.Id)
                            .ToList();

                    return Ok(notifDTOs);
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

        [HttpPut("UpdateNotification/MarkAsRead/{notifId:int}")]
        public async Task<IActionResult> UpdateNotificationMarkAsRead(int notifId)
        {
            try
            {
                if (notifId <= 0) return BadRequest();

                Notification? notification = await _context.Notifications.FirstOrDefaultAsync(notif => notif.Id == notifId);

                if (notification is null) return NotFound();

                notification.IsRead = true;

                // save notif to DB
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateNotification/MarkAllAsRead")]
        public async Task<IActionResult> UpdateNotificationMarkAllAsRead([FromQuery] string userId)
        {
            try
            {
                // find logged in user details
                ApplicationUser? currentUser = await _userManager.FindByIdAsync(userId);

                if (currentUser is null) return BadRequest();

                List<Notification> notifications = _context.Notifications.Where(notif => notif.RecipientId.Equals(userId)).ToList();

                notifications = notifications.Select(notif =>
                {
                    notif.IsRead = true;
                    return notif;
                }).ToList();

                // save notif to DB
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

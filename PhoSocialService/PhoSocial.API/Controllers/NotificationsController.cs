using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoSocial.API.Repositories;
using System;
using System.Threading.Tasks;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notifications;

        public NotificationsController(INotificationRepository notifications)
        {
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var notifications = await _notifications.GetNotificationsAsync(
                Guid.Parse(userId),
                page,
                pageSize
            );
            return Ok(notifications);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var count = await _notifications.GetUnreadCountAsync(Guid.Parse(userId));
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var success = await _notifications.MarkAsReadAsync(id, Guid.Parse(userId));
            if (!success) return BadRequest();
            return Ok();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            await _notifications.MarkAllAsReadAsync(Guid.Parse(userId));
            return Ok();
        }
    }
}

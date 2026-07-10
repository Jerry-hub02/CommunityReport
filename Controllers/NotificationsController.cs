using CommunityReport.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityReport.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext context;

        public NotificationsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // View user's notifications
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        // Get unread count (for bell icon badge)
        public IActionResult GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var count = context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Count();

            return Json(new { count = count });
        }

        public IActionResult GetRecentNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    timeAgo = GetTimeAgo(n.CreatedAt)
                })
                .ToList();

            return Json(new { notifications = notifications });
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            return dateTime.ToString("MMM dd");
        }

        // Mark notification as read
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = context.Notifications
                .Where(n => n.Id == id && n.UserId == userId)
                .FirstOrDefault();

            if (notification != null)
            {
                notification.IsRead = true;
                context.SaveChanges();
            }

            return Json(new { success = true });
        }

        // Mark all as read
        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            context.SaveChanges();

            return Json(new { success = true });
        }

        // Delete notification
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = context.Notifications
                .Where(n => n.Id == id && n.UserId == userId)
                .FirstOrDefault();

            if (notification != null)
            {
                context.Notifications.Remove(notification);
                context.SaveChanges();
                return Json(new {success = true}); 
            }

            return Json(new { success = false });
        }
    }
}

using CommunityReport.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityReport.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext context;

        public DashboardController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Display()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = context.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "UserAuthentication");
            }

            // Get statistics
            var totalComplaints = context.Complaints.Count(c => c.UserId == userId);
            var submittedComplaints = context.Complaints.Count(c => c.UserId == userId && c.Status == "Submitted");
            var pendingComplaints = context.Complaints.Count(c => c.UserId == userId && c.Status == "Pending");
            var inProgressComplaints = context.Complaints.Count(c => c.UserId == userId && c.Status == "InProgress");
            var resolvedComplaints = context.Complaints.Count(c => c.UserId == userId && c.Status == "Resolved");

            var unreadNotifications = context.Notifications.Count(n => n.UserId == userId && !n.IsRead);
            var totalNotifications = context.Notifications.Count(n => n.UserId == userId);

            var chatRoomsCount = context.ChatRoomMembers.Count(m => m.UserId == userId);
            var totalMessages = context.ChatMessages.Count(m => m.UserId == userId);

            // Recent complaints
            var recentComplaints = context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToList();

            // Recent notifications
            var recentNotifications = context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            // Recent chat messages
            var recentChatRooms = context.ChatRoomMembers
                .Where(m => m.UserId == userId)
                .Join(context.ChatRooms,
                    member => member.ChatRoomId,
                    room => room.Id,
                    (member, room) => room)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            // Pass data to view
            ViewData["UserName"] = user.UserName;
            ViewData["Email"] = user.Email;
            ViewData["PhoneNumber"] = user.PhoneNumber ?? "Not provided";

            ViewData["TotalComplaints"] = totalComplaints;
            ViewData["SubmittedComplaints"] = submittedComplaints;
            ViewData["PendingComplaints"] = pendingComplaints;
            ViewData["InProgressComplaints"] = inProgressComplaints;
            ViewData["ResolvedComplaints"] = resolvedComplaints;

            ViewData["UnreadNotifications"] = unreadNotifications;
            ViewData["TotalNotifications"] = totalNotifications;

            ViewData["ChatRoomsCount"] = chatRoomsCount;
            ViewData["TotalMessages"] = totalMessages;

            ViewData["RecentComplaints"] = recentComplaints;
            ViewData["RecentNotifications"] = recentNotifications;
            ViewData["RecentChatRooms"] = recentChatRooms;

            return View();
        }
    }
}

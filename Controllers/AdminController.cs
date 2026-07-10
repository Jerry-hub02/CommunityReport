using CommunityReport.Models;
using CommunityReport.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityReport.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        public IActionResult Display()
        {
            var totalComplaints = context.Complaints.Count();
            var pendingComplaints = context.Complaints.Where(c => c.Status == "Pending").Count();
            var resolvedComplaints = context.Complaints.Where(c => c.Status == "Resolved").Count();
            var totalUsers = context.AspNetUsers.Count();

            ViewData["TotalComplaints"] = totalComplaints;
            ViewData["PendingComplaints"] = pendingComplaints;
            ViewData["ResolvedComplaints"] = resolvedComplaints;
            ViewData["TotalUsers"] = totalUsers;

            return View();
        }
        private readonly ApplicationDbContext context; // Replace with your DbContext name

        public AdminController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // View all complaints
        public IActionResult Complaints()
        {
            var complaints = context.Complaints
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(complaints);
        }

        // Change complaint status - GET
        public IActionResult ChangeStatus(int id)
        {
            var complaint = context.Complaints.Find(id);

            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        // Change complaint status - POST
        [HttpPost]
        public IActionResult ChangeStatus(int id, string status)
        {
            var complaint = context.Complaints.Find(id);

            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            context.SaveChanges();

            TempData["Message"] = "Status updated successfully!";
            return RedirectToAction("Complaints");
        }

        // View complaint details
        public IActionResult ComplaintDetails(int id)
        {
            var complaint = context.Complaints.Find(id);

            if (complaint == null)
            {
                return NotFound();
            }

            // Get user details
            var user = context.AspNetUsers
                .Where(u => u.Id == complaint.UserId)
                .FirstOrDefault();

            ViewData["UserEmail"] = user?.Email ?? "Unknown";
            ViewData["UserName"] = user?.UserName ?? "Unknown";

            return View(complaint);
        }

        // Delete complaint
        [HttpPost]
        public IActionResult DeleteComplaint(int id)
        {
            var complaint = context.Complaints.Find(id);

            if (complaint == null)
            {
                return NotFound();
            }

            context.Complaints.Remove(complaint);
            context.SaveChanges();

            TempData["Message"] = "Complaint deleted successfully!";
            return RedirectToAction("Complaints");
        }

        // Send notification to specific user about their complaint
        [HttpGet]
        public IActionResult SendNotification(int complaintId)
        {
            var complaint = context.Complaints.Find(complaintId);

            if (complaint == null)
            {
                return NotFound();
            }

            ViewData["ComplaintId"] = complaintId;
            ViewData["ComplaintTitle"] = complaint.Title;
            ViewData["UserId"] = complaint.UserId;

            return View();
        }

        [HttpPost]
        public IActionResult SendNotification(int complaintId, string title, string message)
        {
            var complaint = context.Complaints.Find(complaintId);

            if (complaint == null)
            {
                return NotFound();
            }

            var notification = new Notification
            {
                UserId = complaint.UserId,
                Title = title,
                Message = message,
                Type = "Complaint",
                ComplaintId = complaintId,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            context.Notifications.Add(notification);
            context.SaveChanges();

            TempData["Message"] = "Notification sent successfully!";
            return RedirectToAction("ComplaintDetails", new { id = complaintId });
        }

        // Send notification to all users
        [HttpGet]
        public IActionResult BroadcastNotification()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BroadcastNotification(string title, string message)
        {
            var allUsers = context.AspNetUsers
                .Select(u => u.Id)
                .ToList();

            foreach (var userId in allUsers)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = "System",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                context.Notifications.Add(notification);
            }

            context.SaveChanges();

            TempData["Message"] = $"Notification sent to {allUsers.Count} users!";
            return RedirectToAction("Complaints", "Admin");
        }
    }
}

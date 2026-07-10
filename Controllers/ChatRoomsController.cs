using CommunityReport.Hubs;
using CommunityReport.Models;
using CommunityReport.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CommunityReport.Controllers
{
    [Authorize]
    public class ChatRoomsController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ApplicationDbContext context;

        public ChatRoomsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            this.context = context;
            _hubContext = hubContext;
        }

        // List all chat rooms user is member
        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chatRooms = context.ChatRoomMembers
                .Where(m => m.UserId == userId)
                .Join(context.ChatRooms,
                    member => member.ChatRoomId,
                    room => room.Id,
                    (member, room) => room)
                .ToList();

            return View(chatRooms);
        }

        // Create new chat room - GET
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // Create new chat room - POST
        [HttpPost]
        public IActionResult Create(ChatRoom chatRoom)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                chatRoom.CreatedById = userId;
                chatRoom.CreatedAt = DateTime.Now;

                context.ChatRooms.Add(chatRoom);
                context.SaveChanges();

                // Add creator as member
                var member = new ChatRoomMember
                {
                    ChatRoomId = chatRoom.Id,
                    UserId = userId,
                    JoinedAt = DateTime.Now
                };
                context.ChatRoomMembers.Add(member);
                context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(chatRoom);
            }
        }

        // Show all available chat rooms
        public IActionResult Browse()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get rooms user is NOT a member of
            var memberRoomIds = context.ChatRoomMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.ChatRoomId)
                .ToList();

            var availableRooms = context.ChatRooms
                .Where(r => !memberRoomIds.Contains(r.Id))
                .ToList();

            return View(availableRooms);
        }

        // Join a room
        [HttpPost]
        public IActionResult JoinRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if already a member
            var alreadyMember = context.ChatRoomMembers
                .Where(m => m.ChatRoomId == roomId && m.UserId == userId)
                .Any();

            if (!alreadyMember)
            {
                var member = new ChatRoomMember
                {
                    ChatRoomId = roomId,
                    UserId = userId,
                    JoinedAt = DateTime.Now
                };
                context.ChatRoomMembers.Add(member);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Delete chat room (only creator can delete)
        [HttpPost]
        public IActionResult DeleteRoom(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chatRoom = context.ChatRooms
                .Where(r => r.Id == id && r.CreatedById == userId)
                .FirstOrDefault();

            if (chatRoom == null)
            {
                TempData["Error"] = "You can only delete chat rooms you created.";
                return RedirectToAction("Index");
            }

            // Delete all messages in the room
            var messages = context.ChatMessages.Where(m => m.ChatRoomId == id).ToList();
            context.ChatMessages.RemoveRange(messages);

            // Delete all members
            var members = context.ChatRoomMembers.Where(m => m.ChatRoomId == id).ToList();
            context.ChatRoomMembers.RemoveRange(members);

            // Delete the room
            context.ChatRooms.Remove(chatRoom);
            context.SaveChanges();

            TempData["Message"] = "Chat room deleted successfully!";
            return RedirectToAction("Index");
        }

        // Leave chat room
        [HttpPost]
        public IActionResult LeaveRoom(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var member = context.ChatRoomMembers
                .Where(m => m.ChatRoomId == id && m.UserId == userId)
                .FirstOrDefault();

            if (member != null)
            {
                context.ChatRoomMembers.Remove(member);
                context.SaveChanges();

                TempData["Message"] = "You have left the chat room.";
            }

            return RedirectToAction("Index");
        }

        // View chat room and messages
        [Authorize]
        public IActionResult Chat(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is member
            var isMember = context.ChatRoomMembers
                .Where(m => m.ChatRoomId == id && m.UserId == userId)
                .Any();

            if (!isMember)
            {
                return RedirectToAction("Index");
            }

            var chatRoom = context.ChatRooms.Find(id);
            var messages = context.ChatMessages
                .Where(m => m.ChatRoomId == id)
                .OrderBy(m => m.SentAt)
                .ToList();

            ViewData["ChatRoomId"] = id;
            ViewData["ChatRoomName"] = chatRoom.Name;
            ViewData["Messages"] = messages;

            return View();
        }

        // Send message - POST
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatRoomId, string message)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get user from AspNetUsers table
                var currentUser = context.AspNetUsers
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var username = currentUser.UserName ?? currentUser.Email;

                // Check if user is member
                var isMember = context.ChatRoomMembers
                    .Where(m => m.ChatRoomId == chatRoomId && m.UserId == userId)
                    .Any();

                if (!isMember)
                {
                    return Json(new { success = false, message = "Not a member" });
                }

                var chatMessage = new ChatMessage
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    Username = username,
                    Message = message,
                    SentAt = DateTime.Now
                };

                context.ChatMessages.Add(chatMessage);
                context.SaveChanges();

                // Send real-time update to all users in the room
                await _hubContext.Clients.Group(chatRoomId.ToString())
                    .SendAsync("ReceiveMessage", username, message, DateTime.Now.ToString("hh:mm tt"));

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

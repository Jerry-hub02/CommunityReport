using CommunityReport.Models.Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityReport.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext context;

        public ProfileController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // View profile
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = context.AspNetUsers
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Edit profile - GET
        public IActionResult Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = context.AspNetUsers
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Edit profile - POST
        [HttpPost]
        public async Task<IActionResult> Edit(string userName, string email, string phoneNumber)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = context.AspNetUsers
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            // Check if username is already taken
            if (userName != user.UserName)
            {
                var existingUser = context.AspNetUsers
                    .Where(u => u.UserName == userName && u.Id != userId)
                    .FirstOrDefault();

                if (existingUser != null)
                {
                    TempData["Error"] = "Username is already taken.";
                    return RedirectToAction("Edit");
                }
            }

            // Check if email is already taken
            if (email != user.Email)
            {
                var existingEmail = context.AspNetUsers
                    .Where(u => u.Email == email && u.Id != userId)
                    .FirstOrDefault();

                if (existingEmail != null)
                {
                    TempData["Error"] = "Email is already registered.";
                    return RedirectToAction("Edit");
                }
            }

            // Store old username for comparison
            var oldUsername = user.UserName;

            // Update user information
            user.UserName = userName;
            user.Email = email;
            user.NormalizedUserName = userName.ToUpper();
            user.NormalizedEmail = email.ToUpper();
            user.PhoneNumber = phoneNumber;

            context.SaveChanges();

            // If username changed, refresh authentication
            if (oldUsername != userName)
            {
                // Sign out
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Sign back in with new username
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        // Change password - GET
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Change password - POST
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("ChangePassword");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation password do not match.";
                return RedirectToAction("ChangePassword");
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "Password must be at least 6 characters long.";
                return RedirectToAction("ChangePassword");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = context.AspNetUsers
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ChangePassword");
            }

            // Verify current password
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "Current password is incorrect. Please try again.";
                return RedirectToAction("ChangePassword");
            }

            // Hash and set new password
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();

            context.SaveChanges();

            TempData["Message"] = "Password changed successfully! You can now use your new password.";
            return RedirectToAction("Index");
        }

        // Delete account - GET
        public IActionResult DeleteAccount()
        {
            return View();
        }

        // Delete account - POST
        [HttpPost]
        public IActionResult DeleteAccountConfirm(string password)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = context.AspNetUsers
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            // Verify password
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "Incorrect password.";
                return RedirectToAction("DeleteAccount");
            }

            // Delete user's data
            var complaints = context.Complaints.Where(c => c.UserId == userId).ToList();
            context.Complaints.RemoveRange(complaints);

            var notifications = context.Notifications.Where(n => n.UserId == userId).ToList();
            context.Notifications.RemoveRange(notifications);

            var chatMembers = context.ChatRoomMembers.Where(m => m.UserId == userId).ToList();
            context.ChatRoomMembers.RemoveRange(chatMembers);

            var messages = context.ChatMessages.Where(m => m.UserId == userId).ToList();
            context.ChatMessages.RemoveRange(messages);

            // Delete user
            context.AspNetUsers.Remove(user);
            context.SaveChanges();

            // Redirect to Login page (change to your actual login route)
            TempData["Message"] = "Your account has been deleted successfully.";
            return RedirectToAction("Login", "UserAuthentication");
        }
    }
}

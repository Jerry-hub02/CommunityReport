using CommunityReport.Models.Domain.DTO;
using CommunityReport.Repositories.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityReport.Controllers
{
    public class UserAuthentication : Controller
    {
        private readonly IUserAuthenticationService _service;
        public UserAuthentication(IUserAuthenticationService service)
        {
            this._service = service;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            model.Role = "user";
            var result = await _service.RegistrationAsync(model);
            TempData["msg"] = result.Message;
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.LoginAsync(model);
            if (result.StatusCode == 1)
            {
                return RedirectToAction("Display", "Dashboard");
            }
            else
            {
                TempData["msg"] = result.Message;
                return RedirectToAction(nameof(Login));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            //TempData["Message"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Reg()
        {
            var model = new RegistrationModel
            {
                Username = "Admin",
                Name = "John Doe",
                Email = "admin@gmail.com",
                Password = "Admin123@"
            };
            model.Role = "admin";
            var result = await _service.RegistrationAsync(model);
            return Ok(result);
        }
    }
}

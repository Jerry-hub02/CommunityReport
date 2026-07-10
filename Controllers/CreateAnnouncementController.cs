using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityReport.Controllers
{
    public class CreateAnnouncementController : Controller
    {
        [Authorize]
        public IActionResult Display()
        {
            return View();
        }
    }
}

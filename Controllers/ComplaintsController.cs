using CommunityReport.Models;
using CommunityReport.Models.Domain;
using CommunityReport.Models.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace CommunityReport.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ComplaintsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var complaints = context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(p => p.Id)
                .ToList();
            return View(complaints);
        }

        [Authorize]
        public IActionResult Create()
        {
            
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(ComplaintDto complaintDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(!ModelState.IsValid)
            {
                return View(complaintDto);
            }

            // save the image file
            string newFileName = null;
            if (complaintDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(complaintDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/Complaint/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    complaintDto.ImageFile.CopyTo(stream);
                }
            }

            // save the new complaint in the database
            Complaint complaint = new Complaint()
            {
                UserId = userId,
                Type = complaintDto.Type,
                Description = complaintDto.Description,
                Title = complaintDto.Title,
                ImagePath = newFileName,
                Status = "Submitted",
                CreatedAt = DateTime.Now,
            };

            context.Complaints.Add(complaint);
            context.SaveChanges();

            return RedirectToAction("Index", "Complaints");
        }

        [Authorize]
        public IActionResult Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var complaint = context.Complaints
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (complaint == null)
            {
                return RedirectToAction("Index", "Complaints");
            }

            // create complaintDto from complaint
            var complaintDto = new ComplaintDto()
            {
                Type = complaint.Type,
                Description = complaint.Description,
                Title = complaint.Title,

            };

            ViewData["ComplaintId"] = complaint.Id;
            ViewData["ImagePath"] = complaint.ImagePath;
            ViewData["CreatedAt"] = complaint.CreatedAt.ToString("MM/dd/yyyy");

            return View(complaintDto);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Edit(int id, ComplaintDto complaintDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var complaint = context.Complaints
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (complaint == null)
            {
                return RedirectToAction("Index", "Complaints");
            }

            if(!ModelState.IsValid)
            {
                ViewData["ComplaintId"] = complaint.Id;
                ViewData["ImagePath"] = complaint.ImagePath;
                ViewData["CreatedAt"] = complaint.CreatedAt.ToString("MM/dd/yyyy");

                return View(complaintDto);
            }

            // update the image file if we have a new image file
            string newFileName = complaint.ImagePath;
            if(complaintDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(complaintDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/Complaint/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    complaintDto.ImageFile.CopyTo(stream);
                }

                // delete the old image
                string oldImageFullPath = environment.WebRootPath + "/Complaint/" + complaint.ImagePath;
                System.IO.File.Delete(oldImageFullPath);
            }

            // update the product in the database
            complaint.Type = complaintDto.Type;
            complaint.Description = complaintDto.Description;
            complaint.Title = complaintDto.Title;
            complaint.ImagePath = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index", "Complaints");
        }

        // View complaint details
        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the complaint and verify ownership
            var complaint = context.Complaints
                .Where(c => c.Id == id && c.UserId == userId)
                .FirstOrDefault();

            if (complaint == null)
            {
                TempData["Error"] = "Complaint not found or you don't have permission to view it.";
                return RedirectToAction("Index");
            }

            return View(complaint);
        }

        [Authorize]
        public IActionResult Delete(int id)
        {
            var complaint = context.Complaints.Find(id);
            if(complaint == null)
            {
                return RedirectToAction("Index", "Complaints");
            }

            string imageFullPath = environment.WebRootPath + "/Complaint/" + complaint.ImagePath;
            System.IO.File.Delete(imageFullPath);

            context.Complaints.Remove(complaint);
            context.SaveChanges();

            return RedirectToAction("Index", "Complaints");
        }
    }
}

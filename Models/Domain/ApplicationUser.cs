using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityReport.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string? ProfilePicture { get; set; }

        public ICollection<Complaint> Complaints { get; set; }
    }
}

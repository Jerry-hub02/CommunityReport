using CommunityReport.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace CommunityReport.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [MaxLength(100)]
        public string Type { get; set; } = "";

        [MaxLength(100)]
        public string? ImagePath { get; set; } 

        public string Status { get; set; } = "Submitted";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } 
        }

    public enum ComplaintType
    {
        Sewage,
        Pothole,
        Garbage,
        Other
    }

    public enum ComplaintStatus
    {
        Pending,
        InProgress,
        Resolved,
        Rejected
    }
}

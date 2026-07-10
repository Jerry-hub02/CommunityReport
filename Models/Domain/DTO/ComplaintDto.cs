using System.ComponentModel.DataAnnotations;

namespace CommunityReport.Models.Domain.DTO
{
    public class ComplaintDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = "";

        public IFormFile? ImageFile { get; set; }

        public string Status { get; set; } = "";
    }
}

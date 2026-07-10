using System.ComponentModel.DataAnnotations;

namespace CommunityReport.Models.Domain.DTO
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

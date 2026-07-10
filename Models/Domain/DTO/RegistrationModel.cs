using System.ComponentModel.DataAnnotations;

namespace CommunityReport.Models.Domain.DTO
{
    public class RegistrationModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\\-=\\[\\]{};':\"\"\\\\|,.<>\\/?]).{6,}$", ErrorMessage = "Minimum length 6 and must contain 1 Uppercase, 1 lowercase, 1 special character and 1 digit")]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and password confirm do not match.")]
        public string PasswordConfirm { get; set; }
        public string? Role { get; set; }
    }
}

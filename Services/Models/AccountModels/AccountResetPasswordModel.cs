using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels
{
    public class AccountResetPasswordModel
    {
        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be from 8 to 128 characters")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Confirm password must be from 8 to 128 characters")]
        [Compare("Password", ErrorMessage = "Password and confirm password does not match")]
        public required string ConfirmPassword { get; set; }

        [Required] public required string Token { get; set; }
    }
}
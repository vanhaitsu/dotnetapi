using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels
{
    public class AccountChangePasswordModel
    {
        [Required(ErrorMessage = "Old password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Old password must be from 8 to 128 characters")]
        public required string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "New password must be from 8 to 128 characters")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Confirm password must be from 8 to 128 characters")]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password does not match")]
        public required string ConfirmPassword { get; set; }
    }
}
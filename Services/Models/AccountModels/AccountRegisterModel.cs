using System.ComponentModel.DataAnnotations;
using Repositories.Enums;

namespace Services.Models.AccountModels
{
    public class AccountRegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name must be no more than 50 characters")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name must be no more than 50 characters")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Phone number is required"), Phone(ErrorMessage = "Invalid phone format")]
        [StringLength(15, ErrorMessage = "Phone number must be no more than 15 characters")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must be no more than 256 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be from 8 to 128 characters")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Confirm password must be from 8 to 128 characters")]
        [Compare("Password", ErrorMessage = "Password and confirm password does not match")]
        public required string ConfirmPassword { get; set; }
    }
}
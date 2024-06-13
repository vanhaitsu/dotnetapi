using Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace Repositories.ViewModels.AccountModels
{
	public class AccountRegisterModel
	{
		[Required(ErrorMessage = "FirstName is required")]
		[StringLength(50, ErrorMessage = "FirstName must be no more than 50 characters")]
		public string FirstName { get; set; }
		[Required(ErrorMessage = "LastName is required")]
		[StringLength(50, ErrorMessage = "LastName must be no more than 50 characters")]
		public string LastName { get; set; }
		[Required(ErrorMessage = "Gender is required")]
		[EnumDataType(typeof(Gender), ErrorMessage = "Invalid Gender")]
		public Gender Gender { get; set; }
		[Required(ErrorMessage = "Date of Birth is required")]
		public DateTime DateOfBirth { get; set; }
		[Required(ErrorMessage = "PhoneNumber is required"), Phone(ErrorMessage = "Invalid phone format")]
		[StringLength(15, ErrorMessage = "PhoneNumber must be no more than 15 characters")]
		public string PhoneNumber { get; set; }
		[Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format")]
		[StringLength(256, ErrorMessage = "Email must be no more than 256 characters")]
		public string Email { get; set; }
		[Required(ErrorMessage = "Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be from 8 to 128 characters")]
		public string Password { get; set; }
		[Required(ErrorMessage = "Confirm Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "Confirm Password must be from 8 to 128 characters")]
		[Compare("Password", ErrorMessage = "Password and Confirm Password does not match")]
		public string ConfirmPassword { get; set; }
	}
}

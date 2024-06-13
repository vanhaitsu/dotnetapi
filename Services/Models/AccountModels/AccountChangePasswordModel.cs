using System.ComponentModel.DataAnnotations;

namespace Repositories.ViewModels.AccountModels
{
	public class AccountChangePasswordModel
	{
		[Required(ErrorMessage = "Old Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "Old Password must be from 8 to 128 characters")]
		public string OldPassword { get; set; }
		[Required(ErrorMessage = "New Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "New Password must be from 8 to 128 characters")]
		public string NewPassword { get; set; }
		[Required(ErrorMessage = "Confirm Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "Confirm Password must be from 8 to 128 characters")]
		[Compare("NewPassword", ErrorMessage = "New Password and Confirm Password does not match")]
		public string ConfirmPassword { get; set; }
	}
}

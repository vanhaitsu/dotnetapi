using System.ComponentModel.DataAnnotations;

namespace Repositories.ViewModels.AccountModels
{
	public class AccountResetPasswordModel
	{
		public string Email { get; set; }
		[Required(ErrorMessage = "Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be from 8 to 128 characters")]
		public string Password { get; set; }
		[Required(ErrorMessage = "Confirm Password is required")]
		[StringLength(128, MinimumLength = 8, ErrorMessage = "Confirm Password must be from 8 to 128 characters")]
		[Compare("Password", ErrorMessage = "Password and Confirm Password does not match")]
		public string ConfirmPassword { get; set; }
		[Required]
		public string Token {  get; set; }
	}
}

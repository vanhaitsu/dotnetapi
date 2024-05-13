using System.ComponentModel.DataAnnotations;

namespace Repositories.ViewModels.CommonModels
{
	public class EmailModel
	{
		[Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format")]
		[StringLength(256, ErrorMessage = "Email must be no more than 256 characters")]
		public string Email { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace Repositories.ViewModels.CommonModels
{
	public class LoginGoogleIdTokenModel
	{
		[Required(ErrorMessage = "Id Token is required")]
		public string IdToken { get; set; }
	}
}

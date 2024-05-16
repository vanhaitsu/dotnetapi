using System.ComponentModel.DataAnnotations;

namespace Repositories.ViewModels.TokenModels
{
	public class RefreshTokenModel
	{
		[Required(ErrorMessage = "Access Token is required")]
		public string AccessToken { get; set; }
	}
}

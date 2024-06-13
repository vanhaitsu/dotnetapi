namespace Repositories.ViewModels.TokenModels
{
	public class TokenModel
	{
		public string AccessToken { get; set; }
		public DateTime AccessTokenExpiryTime { get; set; }
		public string? RefreshToken { get; set; }
	}
}

namespace Services.Models.TokenModels
{
    public class TokenModel
    {
        public required string AccessToken { get; set; }
        public DateTime AccessTokenExpiryTime { get; set; }
        public string? RefreshToken { get; set; }
    }
}
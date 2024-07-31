using Newtonsoft.Json;

namespace Services.Models.CommonModels
{
    public class GoogleUserInformationModel
    {
        [JsonProperty("email")] public string? Email { get; set; }
        [JsonProperty("given_name")] public string? FirstName { get; set; }
        [JsonProperty("family_name")] public string? LastName { get; set; }
        [JsonProperty("picture")] public string? Image { get; set; }
    }
}
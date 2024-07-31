using System.ComponentModel.DataAnnotations;

namespace Services.Models.CommonModels
{
    public class EmailModel
    {
        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must be no more than 256 characters")]
        public required string Email { get; set; }
    }
}
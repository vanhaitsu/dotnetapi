using System.ComponentModel.DataAnnotations;
using Repositories.Enums;

namespace Services.Models.AccountModels;

public class AccountUpdateModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name must be no more than 50 characters")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name must be no more than 50 characters")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Date of Birth is required")]
    public DateTime DateOfBirth { get; set; }

    public string? Address { get; set; }
    public string? Image { get; set; }

    [Required(ErrorMessage = "Phone number is required"), Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(15, ErrorMessage = "Phone number must be no more than 15 characters")]
    public required string PhoneNumber { get; set; }
}
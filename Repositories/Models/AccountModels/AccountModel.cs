using Repositories.Entities;
using Repositories.Enums;

namespace Repositories.Models.AccountModels;

public class AccountModel : BaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? Role { get; set; }
}
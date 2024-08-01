using Repositories.Common;
using Repositories.Enums;
using Services.Common;

namespace Services.Models.AccountModels;

public class AccountFilterModel: PaginationParameter
{
    public string Order { get; set; } = "";
    public bool OrderByDescending { get; set; } = true;
    public Role? Role { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Gender? Gender { get; set; }
    public string? Search { get; set; }
    // protected override int MinPageSize { get; set; } = PaginationConstant.ACCOUNT_MIN_PAGE_SIZE;
    // protected override int MaxPageSize { get; set; } = PaginationConstant.ACCOUNT_MAX_PAGE_SIZE;
}
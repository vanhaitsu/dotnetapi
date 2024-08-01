using Newtonsoft.Json;
using Repositories.Common;

namespace Services.Common;

public class PaginationParameter
{
    protected virtual int MinPageSize { get; set; } = PaginationConstant.DEFAULT_MIN_PAGE_SIZE;
    protected virtual int MaxPageSize { get; set; } = PaginationConstant.DEFAULT_MAX_PAGE_SIZE;
    public int PageIndex { get; set; } = 1;
    private int _pageSize;

    [JsonIgnore]
    public int PageSize
    {
        get { return _pageSize; }
        set { _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < MinPageSize) ? MinPageSize : value; }
    }

    public PaginationParameter()
    {
        _pageSize = MinPageSize;
    }
}
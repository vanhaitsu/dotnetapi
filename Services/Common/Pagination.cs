namespace Services.Common;

public class Pagination<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }

    public Pagination(List<T> list, int currentPage, int pageSize, int totalPages)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalPages / (double)pageSize);
        AddRange(list);
    }
}
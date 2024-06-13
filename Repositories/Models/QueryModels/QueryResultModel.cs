namespace Repositories.Models.QueryModels;

public class QueryResultModel<TEntity> where TEntity : class
{
    public int TotalCount { get; set; }
    public TEntity? Data { get; set; }
}
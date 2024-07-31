using System.Linq.Expressions;
using Repositories.Entities;
using Repositories.Models.QueryModels;

namespace Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetAsync(Guid id, string include = "");
        Task<List<TEntity>> GetAllAsync();

        Task<QueryResultModel<List<TEntity>>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string include = "",
            int? pageIndex = null,
            int? pageSize = null
        );

        Task AddAsync(TEntity entity);
        Task AddRangeAsync(List<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(List<TEntity> entities);
        void SoftDelete(TEntity entity);
        void SoftDeleteRange(List<TEntity> entities);
        void Restore(TEntity entity);
        void RestoreRange(List<TEntity> entities);
        void HardDelete(TEntity entity);
        void HardDeleteRange(List<TEntity> entities);
    }
}
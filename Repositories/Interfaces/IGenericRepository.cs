using Repositories.Entities;

namespace Repositories.Interfaces
{
	public interface IGenericRepository<TEntity> where TEntity : BaseEntity
	{
		Task<TEntity?> GetAsync(Guid id);
		Task<List<TEntity>> GetAllAsync();
		Task AddAsync(TEntity entity);
		Task AddRangeAsync(List<TEntity> entities);
		void Update(TEntity entity);
		void UpdateRange(List<TEntity> entities);
		void SoftDelete(TEntity entity);
		void SoftDeleteRange(List<TEntity> entities);
		void HardDelete(TEntity entity);
		void HardDeleteRange(List<TEntity> entities);
	}
}

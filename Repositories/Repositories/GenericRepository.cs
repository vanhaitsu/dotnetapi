using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
	{
		protected DbSet<TEntity> _dbSet;
		private readonly IClaimsService _claimsService;

		public GenericRepository(AppDbContext dbContext, IClaimsService claimsService)
		{
			_dbSet = dbContext.Set<TEntity>();
			_claimsService = claimsService;
		}

		public async Task<TEntity?> GetAsync(Guid id)
		{
			var result = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
			// todo should throw exception when result is not found
			return result;
		}

		public Task<List<TEntity>> GetAllAsync()
		{
			return _dbSet.ToListAsync();
		}

		public async Task AddAsync(TEntity entity)
		{
			entity.CreationDate = DateTime.UtcNow;
			entity.CreatedBy = _claimsService.GetCurrentUserId;
			await _dbSet.AddAsync(entity);
		}

		public async Task AddRangeAsync(List<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				entity.CreationDate = DateTime.UtcNow;
				entity.CreatedBy = _claimsService.GetCurrentUserId;
			}
			await _dbSet.AddRangeAsync(entities);
		}
		public void Update(TEntity entity)
		{
			entity.ModificationDate = DateTime.UtcNow;
			entity.ModifiedBy = _claimsService.GetCurrentUserId;
			_dbSet.Update(entity);
		}

		public void UpdateRange(List<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				entity.ModificationDate = DateTime.UtcNow;
				entity.ModifiedBy = _claimsService.GetCurrentUserId;
			}
			_dbSet.UpdateRange(entities);
		}

		public void SoftDelete(TEntity entity)
		{
			entity.IsDeleted = true;
			entity.DeletionDate = DateTime.UtcNow;
			entity.DeletedBy = _claimsService.GetCurrentUserId;
			_dbSet.Update(entity);
		}

		public void SoftDeleteRange(List<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				entity.IsDeleted = true;
				entity.DeletionDate = DateTime.UtcNow;
				entity.DeletedBy = _claimsService.GetCurrentUserId;
			}
			_dbSet.UpdateRange(entities);
		}

		public void HardDelete(TEntity entity)
		{
			_dbSet.Remove(entity);
		}

		public void HardDeleteRange(List<TEntity> entities)
		{
			_dbSet.RemoveRange(entities);
		}
	}
}

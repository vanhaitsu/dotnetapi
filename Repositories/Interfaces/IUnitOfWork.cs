namespace Repositories.Interfaces
{
	public interface IUnitOfWork
	{
		AppDbContext DbContext { get; }
		IAccountRepository AccountRepository { get; }

		public Task<int> SaveChangeAsync();
	}
}

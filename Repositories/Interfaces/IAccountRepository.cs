using System.Linq.Expressions;
using Repositories.Entities;
using Repositories.Models.AccountModels;
using Repositories.Models.QueryModels;

namespace Repositories.Interfaces
{
	public interface IAccountRepository
	{
		Task<QueryResultModel<List<AccountModel>>> GetAllAsync(
			Expression<Func<AccountModel, bool>>? filter = null,
			Func<IQueryable<AccountModel>, IOrderedQueryable<AccountModel>>? orderBy = null,
			string include = "",
			int? pageIndex = null,
			int? pageSize = null
		);
	}
}

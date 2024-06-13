using AutoMapper;
using Repositories.Entities;
using Repositories.ViewModels.AccountModels;

namespace Repositories.Common
{
	public class MapperProfile : Profile
	{
		public MapperProfile()
		{
			//Account
			CreateMap<AccountRegisterModel, Account>();
		}
	}
}

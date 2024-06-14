using AutoMapper;
using Repositories.Entities;
using Repositories.ViewModels.AccountModels;
using Services.Models.CommonModels;

namespace Repositories.Common
{
	public class MapperProfile : Profile
	{
		public MapperProfile()
		{
			//Account
			CreateMap<AccountRegisterModel, Account>();
			CreateMap<GoogleUserInformationModel, Account>();
		}
	}
}

using AutoMapper;
using Repositories.Entities;
using Repositories.Models.AccountModels;
using Services.Models.AccountModels;
using Services.Models.CommonModels;

namespace Services.Common
{
	public class MapperProfile : Profile
	{
		public MapperProfile()
		{
			//Account
			CreateMap<AccountRegisterModel, Account>();
			CreateMap<GoogleUserInformationModel, Account>();
			CreateMap<AccountModel, Account>().ReverseMap();
		}
	}
}

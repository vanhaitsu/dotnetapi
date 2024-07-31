using AutoMapper;
using Repositories.Entities;
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
		}
	}
}

using Services.Models.AccountModels;
using Services.Models.CommonModels;
using Services.Models.ResponseModels;
using Services.Models.TokenModels;

namespace Services.Interfaces
{
	public interface IAccountService
	{
		Task<ResponseModel> Register(AccountRegisterModel accountRegisterModel);
		Task<ResponseDataModel<TokenModel>> Login(AccountLoginModel accountLoginModel);
		Task<ResponseDataModel<TokenModel>> RefreshToken(RefreshTokenModel refreshTokenModel);
		Task<ResponseModel> VerifyEmail(string email, string verificationCode);
		Task<ResponseModel> ResendVerificationEmail(EmailModel? emailModel);
		Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel);
		Task<ResponseModel> ForgotPassword(EmailModel emailModel);
		Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel);
		Task<ResponseDataModel<TokenModel>> LoginGoogle(string code);
	}
}

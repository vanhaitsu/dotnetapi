using Repositories.ViewModels.AccountModels;
using Repositories.ViewModels.CommonModels;
using Repositories.ViewModels.ResponseModels;
using Repositories.ViewModels.TokenModels;

namespace Services.Interfaces
{
	public interface IAccountService
	{
		Task<ResponseModel> Register(AccountRegisterModel accountRegisterModel);
		Task<ResponseDataModel<TokenModel>> Login(AccountLoginModel accountLoginModel);
		Task<ResponseDataModel<TokenModel>> RefreshToken(RefreshTokenModel refreshTokenModel, string refreshTokenFromCookie);
		Task<ResponseModel> VerifyEmail(string email, string verificationCode);
		Task<ResponseModel> ResendVerificationEmail(EmailModel? emailModel);
		Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel);
		Task<ResponseModel> ForgotPassword(EmailModel emailModel);
		Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel);
		Task<ResponseDataModel<TokenModel>> LoginGoogle(LoginGoogleIdTokenModel loginGoogleIdTokenModel);
	}
}

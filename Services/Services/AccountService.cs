using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Utils;
using Repositories.ViewModels.AccountModels;
using Repositories.ViewModels.CommonModels;
using Repositories.ViewModels.ResponseModels;
using Repositories.ViewModels.TokenModels;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Services.Services
{
	public class AccountService : IAccountService
	{
		private readonly UserManager<Account> _userManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;
		private readonly IEmailService _emailService;
		private readonly IClaimsService _claimsService;

		public AccountService(UserManager<Account> userManager, IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IEmailService emailService, IClaimsService claimsService)
		{
			_userManager = userManager;
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_configuration = configuration;
			_emailService = emailService;
			_claimsService = claimsService;
		}

		public async Task<ResponseModel> Register(AccountRegisterModel accountRegisterModel)
		{
			// Check if Email already exists
			var existedEmail = await _userManager.FindByEmailAsync(accountRegisterModel.Email);

			if (existedEmail != null)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "Email already exists"
				};
			}

			// Create new Account
			var user = _mapper.Map<Account>(accountRegisterModel);
			user.UserName = user.Email;
			user.VerificationCode = AuthenticationTools.GenerateVerificationCode(6);
			user.VerificationCodeExpiryTime = DateTime.Now.AddMinutes(15);

			var result = await _userManager.CreateAsync(user, accountRegisterModel.Password);

			if (result.Succeeded)
			{
				// Email verification (Disable this function if Users are not required to verify their Email)
				await SendVerificationEmail(user);

				return new ResponseModel
				{
					Status = true,
					Message = "Account has been created successfully, please verify your Email",
					EmailVerificationRequired = true
				};
			}

			return new ResponseModel
			{
				Status = false,
				Message = "Cannot create Account"
			};
		}

		private async Task SendVerificationEmail(Account account)
		{
			await _emailService.SendEmailAsync(account.Email, "Verify your Email", $"Your verification code is {account.VerificationCode}. The code will expire in 15 minutes.", true);
		}

		public async Task<ResponseDataModel<TokenModel>> Login(AccountLoginModel accountLoginModel)
		{
			var user = await _userManager.FindByNameAsync(accountLoginModel.Email);

			if (user != null && await _userManager.CheckPasswordAsync(user, accountLoginModel.Password))
			{
				var authClaims = new List<Claim>
				{
					new Claim("userId", user.Id.ToString()),
					new Claim("userEmail", user.Email.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				};

				var userRoles = await _userManager.GetRolesAsync(user);

				foreach (var userRole in userRoles)
				{
					authClaims.Add(new Claim(ClaimTypes.Role, userRole));
				}

				// Check if Refresh Token is expired, if so then update
				if (user.RefreshToken == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
				{
					var refreshToken = TokenTools.GenerateRefreshToken();
					_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

					// Update User's Refresh Token
					user.RefreshToken = refreshToken;
					user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

					var result = await _userManager.UpdateAsync(user);

					if (!result.Succeeded)
					{
						return new ResponseDataModel<TokenModel>
						{
							Status = false,
							Message = "Cannot login"
						};
					}
				}

				var jwtToken = TokenTools.CreateJWTToken(authClaims, _configuration);

				return new ResponseDataModel<TokenModel>
				{
					Status = true,
					Message = "Login successfully",
					EmailVerificationRequired = !user.EmailConfirmed,
					Data = new TokenModel
					{
						AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
						AccessTokenExpiryTime = jwtToken.ValidTo.ToLocalTime(),
						RefreshToken = user.RefreshToken,
					}
				};
			}

			return new ResponseDataModel<TokenModel>
			{
				Status = false,
				Message = "Cannot login"
			};
		}

		public async Task<ResponseDataModel<TokenModel>> RefreshToken(RefreshTokenModel refreshTokenModel, string refreshTokenFromCookie)
		{
			// Validate Access Token and Refresh Token
			var principal = TokenTools.GetPrincipalFromExpiredToken(refreshTokenModel.AccessToken, _configuration);

			if (principal == null)
			{
				return new ResponseDataModel<TokenModel>
				{
					Status = false,
					Message = "Invalid Access Token or Refresh Token"
				};
			}

			var user = await _userManager.FindByIdAsync(principal.FindFirst("userId").Value);

			if (user == null || user.RefreshToken != refreshTokenFromCookie || user.RefreshTokenExpiryTime <= DateTime.Now)
			{
				return new ResponseDataModel<TokenModel>
				{
					Status = false,
					Message = "Invalid Access Token or Refresh Token"
				};
			}

			// Start to refresh Access Token and Refresh Token
			var refreshToken = TokenTools.GenerateRefreshToken();
			_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

			// Update User's Refresh Token
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				return new ResponseDataModel<TokenModel>
				{
					Status = false,
					Message = "Cannot refresh the Token"
				};
			}

			var jwtToken = TokenTools.CreateJWTToken(principal.Claims.ToList(), _configuration);

			return new ResponseDataModel<TokenModel>
			{
				Status = true,
				Message = "Refresh Token successfully",
				Data = new TokenModel
				{
					AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
					AccessTokenExpiryTime = jwtToken.ValidTo.ToLocalTime(),
					RefreshToken = user.RefreshToken,
				}
			};
		}

		public async Task<ResponseModel> VerifyEmail(string email, string verificationCode)
		{
			var user = await _userManager.FindByEmailAsync(email);

			if (user == null)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "User not found"
				};
			}

			if (user.VerificationCodeExpiryTime < DateTime.UtcNow)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "The code is expired",
					EmailVerificationRequired = false
				};
			}

			if (user.VerificationCode == verificationCode)
			{
				user.EmailConfirmed = true;
				user.VerificationCode = null;
				user.VerificationCodeExpiryTime = null;

				var result = await _userManager.UpdateAsync(user);

				if (result.Succeeded)
				{
					return new ResponseModel
					{
						Status = true,
						Message = "Verify Email successfully",
					};
				}
			}

			return new ResponseModel
			{
				Status = false,
				Message = "Cannot verify Email",
			};
		}

		public async Task<ResponseModel> ResendVerificationEmail(EmailModel emailModel)
		{
			var currentUserId = _claimsService.GetCurrentUserId;

			if (emailModel == null && currentUserId == null)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "User not found",
				};
			}

			Account user = null;

			if (emailModel != null && currentUserId == null)
			{
				user = await _userManager.FindByEmailAsync(emailModel.Email);

				if (user == null)
				{
					return new ResponseModel
					{
						Status = false,
						Message = "User not found",
					};
				}
			}
			else if (emailModel == null && currentUserId != null)
			{
				user = await _userManager.FindByIdAsync(currentUserId.ToString());
			}
			else if (emailModel != null && currentUserId != null)
			{
				user = await _userManager.FindByEmailAsync(emailModel.Email);

				if (user == null || user.Id != currentUserId)
				{
					return new ResponseModel
					{
						Status = false,
						Message = "Cannot resend Verification Email",
						EmailVerificationRequired = true
					};
				}
			}

			if (user.EmailConfirmed)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "Email has been verified",
				};
			}

			// Update new Verification IdToken
			user.VerificationCode = AuthenticationTools.GenerateVerificationCode(6);
			user.VerificationCodeExpiryTime = DateTime.Now.AddMinutes(15);
			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				await SendVerificationEmail(user);

				return new ResponseModel
				{
					Status = true,
					Message = "Resend Verification Email successfully",
					EmailVerificationRequired = true
				};
			}

			return new ResponseModel
			{
				Status = false,
				Message = "Cannot resend Verification Email",
				EmailVerificationRequired = true
			};
		}

		public async Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel)
		{
			var currentUserId = _claimsService.GetCurrentUserId;
			var user = await _userManager.FindByIdAsync(currentUserId.ToString());

			var result = await _userManager.ChangePasswordAsync(user, accountChangePasswordModel.OldPassword, accountChangePasswordModel.NewPassword);

			if (result.Succeeded)
			{
				return new ResponseModel
				{
					Status = true,
					Message = "Change Password successfully",
				};
			}

			return new ResponseModel
			{
				Status = false,
				Message = "Cannot change Password",
			};
		}

		public async Task<ResponseModel> ForgotPassword(EmailModel emailModel)
		{
			var user = await _userManager.FindByEmailAsync(emailModel.Email);

			if (user == null)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "User not found",
				};
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			// todo modify this Email body to send a URL redirect to the frontend page and contain the token as a parameter in the URL
			await _emailService.SendEmailAsync(user.Email, "Reset your Password", $"Your token is {token}. The token will expire in 15 minutes.", true);

			return new ResponseModel
			{
				Status = true,
				Message = "An Email has been sent, please check your inbox",
			};
		}

		public async Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel)
		{
			var user = await _userManager.FindByEmailAsync(accountResetPasswordModel.Email);

			if (user == null)
			{
				return new ResponseModel
				{
					Status = false,
					Message = "User not found",
				};
			}

			var result = await _userManager.ResetPasswordAsync(user, accountResetPasswordModel.Token, accountResetPasswordModel.Password);

			if (result.Succeeded)
			{
				return new ResponseModel
				{
					Status = true,
					Message = "Reset Password successfully",
				};
			}

			return new ResponseModel
			{
				Status = false,
				Message = "Cannot reset Password",
			};
		}

		public async Task<ResponseDataModel<TokenModel>> LoginGoogle(LoginGoogleIdTokenModel loginGoogleIdTokenModel)
		{
			var settings = new GoogleJsonWebSignature.ValidationSettings()
			{
				Audience = new List<string> { _configuration["OAuth2:Google:ClientId"] }
			};

			var payload = await GoogleJsonWebSignature.ValidateAsync(loginGoogleIdTokenModel.IdToken, settings);

			if (payload == null)
			{
				return new ResponseDataModel<TokenModel>
				{
					Status = false,
					Message = "Invalid credentials",
				};
			}

			// Use payload based on need
			var user = await _userManager.FindByEmailAsync(payload.Email);

			if (user == null)
			{
				return new ResponseDataModel<TokenModel>
				{
					Status = false,
					Message = "User not found"
				};
			}

			// JWT Token
			var authClaims = new List<Claim>
				{
					new Claim("userId", user.Id.ToString()),
					new Claim("userEmail", user.Email.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				};

			var userRoles = await _userManager.GetRolesAsync(user);

			foreach (var userRole in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, userRole));
			}

			// Check if Refresh Token is expired, if so then update
			if (user.RefreshToken == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
			{
				var refreshToken = TokenTools.GenerateRefreshToken();
				_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

				// Update User's Refresh Token
				user.RefreshToken = refreshToken;
				user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

				var result = await _userManager.UpdateAsync(user);

				if (!result.Succeeded)
				{
					return new ResponseDataModel<TokenModel>
					{
						Status = false,
						Message = "Cannot login"
					};
				}
			}

			var jwtToken = TokenTools.CreateJWTToken(authClaims, _configuration);

			return new ResponseDataModel<TokenModel>
			{
				Status = true,
				Message = "Login successfully",
				EmailVerificationRequired = !user.EmailConfirmed,
				Data = new TokenModel
				{
					AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
					AccessTokenExpiryTime = jwtToken.ValidTo.ToLocalTime(),
					RefreshToken = user.RefreshToken,
				}
			};
		}
	}
}

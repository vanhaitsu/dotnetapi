using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Utils;
using Repositories.ViewModels.AccountModels;
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
			_ = int.TryParse(_configuration["VerificationCode:CodeValidityInMinutes"], out int codeValidityInMinutes);
			user.VerificationCodeExpiryTime = DateTime.Now.AddDays(codeValidityInMinutes);

			var result = await _userManager.CreateAsync(user, accountRegisterModel.Password);

			if (result.Succeeded)
			{
				// Email verification
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
				if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
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

		public async Task<ResponseDataModel<TokenModel>> RefreshToken(RefreshTokenModel refreshTokenModel)
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

			if (user == null || user.RefreshToken != refreshTokenModel.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
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
					Message = "The Code is expired",
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

		public async Task<ResponseModel> ResendVerificationEmail(string email)
		{
			var currentUserId = _claimsService.GetCurrentUserId;

			if (email == null && currentUserId == null)
			{
				return new ResponseModel
				{
					Status = true,
					Message = "User not found",
				};
			}

			Account user = null;

			if (email != null && currentUserId == null)
			{
				user = await _userManager.FindByEmailAsync(email);

				if (user == null)
				{
					return new ResponseModel
					{
						Status = true,
						Message = "User not found",
					};
				}
			}
			else if (email == null && currentUserId != null)
			{
				user = await _userManager.FindByIdAsync(currentUserId.ToString());
			}

            if (user.EmailConfirmed)
            {
				return new ResponseModel
				{
					Status = true,
					Message = "Email has been verified",
				};
			}

			// Update new Verification Code
			user.VerificationCode = AuthenticationTools.GenerateVerificationCode(6);
			_ = int.TryParse(_configuration["VerificationCode:CodeValidityInMinutes"], out int codeValidityInMinutes);
			user.VerificationCodeExpiryTime = DateTime.Now.AddDays(codeValidityInMinutes);
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
				Status = true,
				Message = "Cannot resend Verification Email",
				EmailVerificationRequired = true
			};
		}
	}
}

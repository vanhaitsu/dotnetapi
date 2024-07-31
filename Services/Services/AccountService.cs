using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Utils;
using Services.Interfaces;
using Services.Models.CommonModels;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Services.Models.AccountModels;
using Services.Models.ResponseModels;
using Services.Models.TokenModels;

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

        public AccountService(UserManager<Account> userManager, IUnitOfWork unitOfWork, IMapper mapper,
            IConfiguration configuration, IEmailService emailService, IClaimsService claimsService)
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
            // Check if email already exists
            var existedEmail = await _userManager.FindByEmailAsync(accountRegisterModel.Email);

            if (existedEmail != null)
            {
                return new ResponseModel
                {
                    Status = false,
                    Message = "Email already exists"
                };
            }

            // Create new account
            var user = _mapper.Map<Account>(accountRegisterModel);
            user.UserName = user.Email;
            user.VerificationCode = AuthenticationTools.GenerateVerificationCode(6);
            user.VerificationCodeExpiryTime = DateTime.Now.AddMinutes(15);

            var result = await _userManager.CreateAsync(user, accountRegisterModel.Password);

            if (result.Succeeded)
            {
                // Add role
                await _userManager.AddToRoleAsync(user, Repositories.Enums.Role.User.ToString());

                // Email verification (disable this function if users are not required to verify their email)
                await SendVerificationEmail(user);

                return new ResponseModel
                {
                    Status = true,
                    Message = "Account has been created successfully, please verify your email",
                    EmailVerificationRequired = true
                };
            }

            return new ResponseModel
            {
                Status = false,
                Message = "Cannot create account"
            };
        }

        private async Task SendVerificationEmail(Account account)
        {
            await _emailService.SendEmailAsync(account.Email!, "Verify your email",
                $"Your verification code is {account.VerificationCode}. The code will expire in 15 minutes.", true);
        }

        public async Task<ResponseDataModel<TokenModel>> Login(AccountLoginModel accountLoginModel)
        {
            var user = await _userManager.FindByNameAsync(accountLoginModel.Email);

            if (user != null)
            {
                if (user.IsDeleted)
                {
                    return new ResponseDataModel<TokenModel>
                    {
                        Status = false,
                        Message = "Account has been deleted"
                    };
                }

                if (await _userManager.CheckPasswordAsync(user, accountLoginModel.Password))
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim("userId", user.Id.ToString()),
                        new Claim("userEmail", user.Email!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    var userRoles = await _userManager.GetRolesAsync(user);

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    // Check if refresh token is expired, if so then update
                    if (user.RefreshToken == null || user.RefreshTokenExpiryTime < DateTime.Now)
                    {
                        var refreshToken = TokenTools.GenerateRefreshToken();
                        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"],
                            out int refreshTokenValidityInDays);

                        // Update user's refresh token
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

            return new ResponseDataModel<TokenModel>
            {
                Status = false,
                Message = "Cannot login"
            };
        }

        public async Task<ResponseDataModel<TokenModel>> RefreshToken(RefreshTokenModel refreshTokenModel)
        {
            // Validate access token and refresh token
            var principal = TokenTools.GetPrincipalFromExpiredToken(refreshTokenModel.AccessToken, _configuration);

            var user = await _userManager.FindByIdAsync(principal!.FindFirst("userId")!.Value);

            if (user == null || user.RefreshToken != refreshTokenModel.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new ResponseDataModel<TokenModel>
                {
                    Status = false,
                    Message = "Invalid access token or refresh token"
                };
            }

            // Start to refresh access token and refresh token
            var refreshToken = TokenTools.GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            // Update user's refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ResponseDataModel<TokenModel>
                {
                    Status = false,
                    Message = "Cannot refresh the token"
                };
            }

            var jwtToken = TokenTools.CreateJWTToken(principal.Claims.ToList(), _configuration);

            return new ResponseDataModel<TokenModel>
            {
                Status = true,
                Message = "Refresh token successfully",
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

            if (user.VerificationCodeExpiryTime < DateTime.Now)
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
                        Message = "Verify email successfully",
                    };
                }
            }

            return new ResponseModel
            {
                Status = false,
                Message = "Cannot verify email",
            };
        }

        public async Task<ResponseModel> ResendVerificationEmail(EmailModel? emailModel)
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

            Account? user = null;

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
                user = await _userManager.FindByIdAsync(currentUserId.ToString()!);
            }
            else if (emailModel != null && currentUserId != null)
            {
                user = await _userManager.FindByEmailAsync(emailModel.Email);

                if (user == null || user.Id != currentUserId)
                {
                    return new ResponseModel
                    {
                        Status = false,
                        Message = "Cannot resend verification email",
                        EmailVerificationRequired = true
                    };
                }
            }

            if (user!.EmailConfirmed)
            {
                return new ResponseModel
                {
                    Status = false,
                    Message = "Email has been verified",
                };
            }

            // Update new verification code
            user.VerificationCode = AuthenticationTools.GenerateVerificationCode(6);
            user.VerificationCodeExpiryTime = DateTime.Now.AddMinutes(15);
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await SendVerificationEmail(user);

                return new ResponseModel
                {
                    Status = true,
                    Message = "Resend Verification email successfully",
                    EmailVerificationRequired = true
                };
            }

            return new ResponseModel
            {
                Status = false,
                Message = "Cannot resend verification email",
                EmailVerificationRequired = true
            };
        }

        public async Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel)
        {
            var currentUserId = _claimsService.GetCurrentUserId;
            var user = await _userManager.FindByIdAsync(currentUserId.ToString()!);

            var result = await _userManager.ChangePasswordAsync(user!, accountChangePasswordModel.OldPassword,
                accountChangePasswordModel.NewPassword);

            if (result.Succeeded)
            {
                return new ResponseModel
                {
                    Status = true,
                    Message = "Change password successfully",
                };
            }

            return new ResponseModel
            {
                Status = false,
                Message = "Cannot change password",
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
            await _emailService.SendEmailAsync(user.Email!, "Reset your Password",
                $"Your token is {token}. The token will expire in 15 minutes.", true);

            return new ResponseModel
            {
                Status = true,
                Message = "An email has been sent, please check your inbox",
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

            var result = await _userManager.ResetPasswordAsync(user, accountResetPasswordModel.Token,
                accountResetPasswordModel.Password);

            if (result.Succeeded)
            {
                return new ResponseModel
                {
                    Status = true,
                    Message = "Reset password successfully",
                };
            }

            return new ResponseModel
            {
                Status = false,
                Message = "Cannot reset password",
            };
        }

        public async Task<ResponseDataModel<TokenModel>> LoginGoogle(string code)
        {
            // Exchange authorization code for refresh and access tokens
            HttpClient tokenClient = new HttpClient { BaseAddress = new Uri("https://oauth2.googleapis.com/token") };
            tokenClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var googleTokenRequestData = new
            {
                client_id = _configuration["OAuth2:Google:ClientId"],
                client_secret = _configuration["OAuth2:Google:ClientSecret"],
                code,
                grant_type = "authorization_code",
                redirect_uri = _configuration["OAuth2:Server:RedirectURI"] + "/api/v1/authentication/login/google"
            };

            HttpResponseMessage googleTokenResponse = await tokenClient.PostAsJsonAsync("", googleTokenRequestData);

            if (!googleTokenResponse.IsSuccessStatusCode)
            {
                return new ResponseDataModel<TokenModel>
                {
                    Status = false,
                    Message = "Error when trying to connect to Google API"
                };
            }

            // Get user information with Google access token
            var googleTokenModel =
                JsonConvert.DeserializeObject<GoogleTokenModel>(await googleTokenResponse.Content.ReadAsStringAsync());
            var userInfoClient = new HttpClient { BaseAddress = new Uri("https://www.googleapis.com/oauth2/v1/") };
            HttpResponseMessage googleUserInformationResponse =
                await userInfoClient.GetAsync($"userinfo?access_token={googleTokenModel!.AccessToken}");

            if (!googleUserInformationResponse.IsSuccessStatusCode)
            {
                return new ResponseDataModel<TokenModel>
                {
                    Status = false,
                    Message = "Error when trying to connect to Google API"
                };
            }

            var googleUserInformationModel =
                JsonConvert.DeserializeObject<GoogleUserInformationModel>(await googleUserInformationResponse.Content
                    .ReadAsStringAsync());

            // Handle user information
            var user = await _userManager.FindByEmailAsync(googleUserInformationModel!.Email!);

            if (user == null)
            {
                //return new ResponseDataModel<TokenModel>
                //{
                //    Status = false,
                //    Message = "User not found"
                //};

                user = _mapper.Map<Account>(googleUserInformationModel);
                user.UserName = user.Email;
                var saveUserResult = await _userManager.CreateAsync(user);

                if (saveUserResult.Succeeded)
                {
                    // Add role
                    await _userManager.AddToRoleAsync(user, Repositories.Enums.Role.User.ToString());
                }
                else
                {
                    return new ResponseDataModel<TokenModel>
                    {
                        Status = false,
                        Message = "Cannot create account"
                    };
                }
            }

            if (user.IsDeleted)
            {
                return new ResponseDataModel<TokenModel>
                {
                    Status = false,
                    Message = "Account has been deleted"
                };
            }

            // JWT token
            var authClaims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userEmail", user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // Check if refresh token is expired, if so then update
            if (user.RefreshToken == null || user.RefreshTokenExpiryTime < DateTime.Now)
            {
                var refreshToken = TokenTools.GenerateRefreshToken();
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                // Update user's refresh token
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
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.ViewModels.AccountModels;
using Repositories.ViewModels.CommonModels;
using Repositories.ViewModels.TokenModels;
using Services.Interfaces;

namespace API.Controllers
{
	[Route("api/v1/authentication")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AuthenticationController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] AccountRegisterModel accountRegisterModel)
		{
			try
			{
				var result = await _accountService.Register(accountRegisterModel);
				if (result.Status)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] AccountLoginModel accountLoginModel)
		{
			try
			{
				var result = await _accountService.Login(accountLoginModel);
				if (result.Status)
				{
					HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken,
						new CookieOptions
						{
							Expires = DateTimeOffset.UtcNow.AddDays(7),
							HttpOnly = true,
							IsEssential = true,
							Secure = true,
							SameSite = SameSiteMode.None
						});

					result.Data.RefreshToken = null;

					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost("token/refresh")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel refreshTokenModel)
		{
			try
			{
				HttpContext.Request.Cookies.TryGetValue("refreshToken", out string refreshTokenFromCookie);

				if (refreshTokenFromCookie != null)
				{
					refreshTokenModel.RefreshToken = refreshTokenFromCookie;
				}

                var result = await _accountService.RefreshToken(refreshTokenModel);
				if (result.Status)
				{
					if (refreshTokenFromCookie != null)
					{
                        HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(7),
                            HttpOnly = true,
                            IsEssential = true,
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });

                        result.Data.RefreshToken = null;
                    }

					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpGet("email/verify")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string verificationCode)
		{
			try
			{
				var result = await _accountService.VerifyEmail(email, verificationCode);
				if (result.Status)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost("email/resend-verification")]
		public async Task<IActionResult> ResendVerificationEmail([FromBody] EmailModel emailModel)
		{
			try
			{
				var result = await _accountService.ResendVerificationEmail(emailModel);
				if (result.Status)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost("password/change")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] AccountChangePasswordModel accountChangePasswordModel)
		{
			try
			{
				var result = await _accountService.ChangePassword(accountChangePasswordModel);
				if (result.Status)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost("password/forgot")]
		public async Task<IActionResult> ForgotPassword([FromBody] EmailModel emailModel)
		{
			try
			{
				var result = await _accountService.ForgotPassword(emailModel);
				if (result.Status)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost("password/reset")]
		public async Task<IActionResult> ResetPassword([FromBody] AccountResetPasswordModel accountResetPasswordModel)
		{
			try
			{
				var result = await _accountService.ResetPassword(accountResetPasswordModel);
				if (result.Status)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpGet("login/google")]
		public async Task<IActionResult> LoginGoogle([FromQuery] string code)
		{
			try
			{
				var result = await _accountService.LoginGoogle(code);
				if (result.Status)
				{
					//HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken,
					//	new CookieOptions
					//	{
					//		Expires = DateTimeOffset.UtcNow.AddDays(7),
					//		HttpOnly = true,
					//		IsEssential = true,
					//		Secure = true,
					//		SameSite = SameSiteMode.None
					//	});

					//result.Data.RefreshToken = null;

					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}
	}
}

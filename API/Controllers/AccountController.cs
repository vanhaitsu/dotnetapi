using Microsoft.AspNetCore.Mvc;
using Repositories.ViewModels.AccountModels;
using Repositories.ViewModels.TokenModels;
using Services.Interfaces;

namespace API.Controllers
{
	[Route("api/v1/account")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AccountController(IAccountService accountService)
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

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel refreshTokenModel)
		{
			try
			{
				var result = await _accountService.RefreshToken(refreshTokenModel);
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

		[HttpGet("verify-email")]
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

		[HttpGet("resend-verification-email")]
		public async Task<IActionResult> ResendVerificationEmail([FromQuery] string? email)
		{
			try
			{
				var result = await _accountService.ResendVerificationEmail(email);
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
	}
}

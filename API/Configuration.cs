using Repositories.Interfaces;
using Repositories.Common;
using API.Middlewares;
using System.Diagnostics;
using Repositories.Entities;
using Repositories;
using Services.Interfaces;
using Services.Services;
using Repositories.Repositories;
using Microsoft.AspNetCore.Identity;
using Services.Common;
using API.Services;

namespace API
{
	public static class Configuration
	{
		/*
			References
			JWT: https://www.c-sharpcorner.com/article/jwt-authentication-with-refresh-tokens-in-net-6-0
			Email Confirmation: https://webtutorialstack.com/dot-net-framework/email-confirmation-with-asp-net-core-identity/
		*/

		public static IServiceCollection AddAPIConfiguration(this IServiceCollection services)
		{
			// Identity
			services
				.AddIdentity<Account, Role>()
				.AddRoles<Role>()
				.AddEntityFrameworkStores<AppDbContext>()
				.AddDefaultTokenProviders();

			// Middlewares
			services.AddSingleton<GlobalExceptionMiddleware>();
			services.AddSingleton<PerformanceMiddleware>();
			services.AddSingleton<Stopwatch>();

			// Common
			services.AddHttpContextAccessor();
			services.AddAutoMapper(typeof(MapperProfile).Assembly);
			services.AddScoped<IClaimsService, ClaimsService>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddTransient<IEmailService, EmailService>();

			// Dependency Injection
			// Account
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IAccountRepository, AccountRepository>();

			return services;
		}
	}
}

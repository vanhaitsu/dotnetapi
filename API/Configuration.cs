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
		public static IServiceCollection AddAPIConfiguration(this IServiceCollection services)
		{
			// Identity
			services
				.AddIdentity<Account, Role>(options =>
				{
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequiredLength = 8;
				})
				.AddRoles<Role>()
				.AddEntityFrameworkStores<AppDbContext>()
				.AddDefaultTokenProviders();
			services.Configure<DataProtectionTokenProviderOptions>(options =>
			{
				options.TokenLifespan = TimeSpan.FromMinutes(15);
			});

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

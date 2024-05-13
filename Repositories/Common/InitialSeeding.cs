using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Entities;

namespace Repositories.Common
{
	/// <summary>
	/// This class is used to insert initial data
	/// </summary>
	public class InitialSeeding
	{
		private static readonly string[] roles = ["Administrator", "User"];

		public static async Task Initialize(IServiceProvider serviceProvider)
		{
			var _roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

			if (_roleManager != null)
			{
				foreach (string role in roles)
				{
					Role? existedRole = await _roleManager.FindByNameAsync(role);
					if (existedRole == null)
					{
						await _roleManager.CreateAsync(new Role { Name = role });
					}
				}
			}
		}
	}
}

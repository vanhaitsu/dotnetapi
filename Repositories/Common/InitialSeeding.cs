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
        private static readonly string[] RoleList = [Enums.Role.Admin.ToString(), Enums.Role.User.ToString()];

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            foreach (string role in RoleList)
            {
                Role? existedRole = await roleManager.FindByNameAsync(role);
                if (existedRole == null)
                {
                    await roleManager.CreateAsync(new Role { Name = role });
                }
            }
        }
    }
}
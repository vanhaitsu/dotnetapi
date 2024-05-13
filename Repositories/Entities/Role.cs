using Microsoft.AspNetCore.Identity;

namespace Repositories.Entities
{
	public class Role : IdentityRole<Guid>
	{
		public string? Description { get; set; }
	}
}

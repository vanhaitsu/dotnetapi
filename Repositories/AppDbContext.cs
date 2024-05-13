using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
	public class AppDbContext : IdentityDbContext<Account, Role, Guid>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Account>(entity =>
			{
				entity.Property(x => x.FirstName).HasMaxLength(50);
				entity.Property(x => x.LastName).HasMaxLength(50);
				entity.Property(x => x.PhoneNumber).HasMaxLength(15);
				entity.Property(x => x.VerificationCode).HasMaxLength(6);
			});

			modelBuilder.Entity<Role>(entity =>
			{
				entity.Property(x => x.Description).HasMaxLength(256);
			});
		}
	}
}

using System.Security.Claims;
using Repositories.Interfaces;
using Repositories.Utils;

namespace API.Utils
{
    public class ClaimsService : IClaimsService
    {
		public Guid? GetCurrentUserId { get; }

		public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
			GetCurrentUserId = AuthenticationTools.GetCurrentUserId(identity!);
        }
    }
}

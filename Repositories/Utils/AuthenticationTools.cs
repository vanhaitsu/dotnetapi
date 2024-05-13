using System.Security.Claims;
using System.Text;

namespace Repositories.Utils
{
	public class AuthenticationTools
	{
		private static Random random = new Random();

		public static string GenerateVerificationCode(int length)
		{
			const string chars = "0123456789";
			StringBuilder otp = new StringBuilder(length);

			for (int i = 0; i < length; i++)
			{
				otp.Append(chars[random.Next(chars.Length)]);
			}

			return otp.ToString();
		}

		public static Guid? GetCurrentUserId(ClaimsIdentity claimsIdentity)
		{
			if (claimsIdentity != null)
			{
				var userIdClaim = claimsIdentity.FindFirst("userId");

				if (userIdClaim != null)
				{
					return new Guid(userIdClaim.Value);
				}
			}

			return null;
		}
	}
}

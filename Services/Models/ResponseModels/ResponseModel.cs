namespace Repositories.ViewModels.ResponseModels
{
	/// <summary>
	/// Use this model for responses that do not require any additional data
	/// </summary>
	public class ResponseModel
	{
		public bool Status { get; set; } = false;
		public string Message { get; set; }

		// Optional
		public bool? EmailVerificationRequired { get; set; }
		public bool? IsBlocked { get; set; }
	}
}

namespace Repositories.ViewModels.ResponseModels
{
	/// <summary>
	/// Use this model for responses that require additional data
	/// </summary>
	/// <typeparam name="TEntity">Data type of additional data model</typeparam>
	public class ResponseDataModel<TEntity> : ResponseModel where TEntity : class
	{
		public TEntity? Data { get; set; }
	}
}

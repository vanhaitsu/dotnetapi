namespace API.Middlewares
{
	public class GlobalExceptionMiddleware : IMiddleware
	{
		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				// todo push notification and write log
				Console.WriteLine("GlobalExceptionMiddleware");
				Console.WriteLine(ex.Message);
				await context.Response.WriteAsync(ex.ToString());
			}
		}
	}
}

using System.Diagnostics;

namespace API.Middlewares
{
	public class PerformanceMiddleware : IMiddleware
	{
		private readonly Stopwatch stopwatch;

		public PerformanceMiddleware(Stopwatch stopwatch)
		{
			this.stopwatch = stopwatch;
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			stopwatch.Restart();
			stopwatch.Start();
			Console.WriteLine("Start performance recored");
			await next(context);
			Console.WriteLine("End performance recored");
			stopwatch.Stop();
			TimeSpan timeTaken = stopwatch.Elapsed;
			Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
		}
	}
}

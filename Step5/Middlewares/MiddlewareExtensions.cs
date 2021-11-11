using Microsoft.AspNetCore.Builder;

namespace SuperCRM.Middlewares
{
	public static class MiddlewareExtensions
	{

		public static void UseAuthSessionCaching(this IApplicationBuilder app)
		{
			app.UseMiddleware<AuthSessionCachingMiddleware>();
		}
	}
}
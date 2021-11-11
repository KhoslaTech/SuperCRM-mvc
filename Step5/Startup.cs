using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SuperCRM
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			ASPSecurityKitConfiguration.ConfigureServices(services, Configuration);
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ASPSecurityKitConfiguration.ConfigureContainer(builder);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			ASPSecurityKitConfiguration.Configure(app, env);

			app.UseStaticFiles();
			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}

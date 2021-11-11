using ASPSecurityKit;
using ASPSecurityKit.Net;
using ASPSecurityKit.NetCore;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperCRM.DataModels;
using SuperCRM.DependencyInjection;
using SuperCRM.Middlewares;

namespace SuperCRM
{
	public class ASPSecurityKitConfiguration
	{
		public static bool IsDevelopmentEnvironment { get; set; }

		public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddControllersWithViews(options =>
				{
					options.Filters.Add(typeof(ProtectAttribute));
				})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
				});

			services.AddDbContext<AppDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			services.AddHttpContextAccessor();
		}

		public static void ConfigureContainer(ContainerBuilder builder)
		{
			License.TryRegisterFromExecutionPath();

			// Register all ASK components and auth definitions
			new ASPSecurityKitRegistry()
				.Register(new ASKContainerBuilder(builder), authRequestDefinitionType: typeof(AuthDefinitions.AuthDefinitionBase));

			builder.RegisterModule<AppRegistry>();

		}

		public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			IsDevelopmentEnvironment = env.IsDevelopment();

			if (IsDevelopmentEnvironment)
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler();

				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseAuthSessionCaching();

			var settings = app.ApplicationServices.GetService<INetSecuritySettings>();
			settings.MustHaveBeenVerified = false;
		}

	}
}
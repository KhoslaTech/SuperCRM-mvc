using System;
using SuperCRM.AuthDefinitions;
using SuperCRM.DataModels;
using SuperCRM.Repositories;
using SuperCRM.Security;
using ASPSecurityKit;
using ASPSecurityKit.Authorization;
using ASPSecurityKit.AuthProviders;
using Autofac;

namespace SuperCRM.DependencyInjection
{
	public class AppRegistry : Module
	{
		protected override void Load(ContainerBuilder builder)
		{


			builder.RegisterType<Config>()
				.As<IConfig>()
				.SingleInstance();

			builder.RegisterType<Logger>()
				.As<ILogger>()
				.SingleInstance();


			builder.RegisterType<IdentityRepository>()
				.As<IIdentityRepository>()
				.InstancePerLifetimeScope();

			builder.RegisterType<UserRepository>()
				.As<IUserRepository<Guid>>()
				.InstancePerLifetimeScope();

			builder.RegisterType<PermitRepository>()
				.As<IUserPermitRepository>()
				.As<IPermitRepository<Guid, Guid>>()
				.InstancePerLifetimeScope();

			builder.RegisterType<UserService<Guid, Guid, DbUser>>()
				.As<IUserService<Guid, Guid, DbUser>>()
				.As<IUserService>()
				.InstancePerLifetimeScope();

			builder.RegisterType<InMemoryPermitManager<Guid>>()
				.As<IPermitManager<Guid>>()
				.InstancePerLifetimeScope();

			builder.RegisterType<AuthenticationProvider>()
				.As<IAuthenticationProvider>()
				.InstancePerLifetimeScope();

			builder.RegisterType<ServiceKeyHandler>()
				.As<IAuthenticationSchemeHandler>()
				.InstancePerLifetimeScope();

			builder.RegisterType<AuthCookieHandler>()
				.As<IAuthenticationSchemeHandler>()
				.InstancePerLifetimeScope();

			builder.RegisterType<DefaultHmacTokenHandler>()
				.As<IAuthenticationSchemeHandler>()
				.InstancePerLifetimeScope();

			builder.RegisterType<ServiceHmacTokenHandler>()
				.As<IAuthenticationSchemeHandler>()
				.InstancePerLifetimeScope();

			builder.RegisterType<EntityIdAuthorizer<IdMemberReference>>()
				.As<IEntityIdAuthorizer>()
				.InstancePerLifetimeScope();

			builder.RegisterType<ReferencesProvider>()
				.As<IReferencesProvider<IdMemberReference>>()
				.InstancePerLifetimeScope();

			builder.RegisterType<CachedAuthSessionProvider>()
				.As<IAuthSessionProvider>()
				.AsSelf()
				.InstancePerLifetimeScope();


			builder.RegisterType<AppDbContext>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<AjaxSecurityFailureResponseHandler>()
				.As<ISecurityFailureResponseHandler>()
				.SingleInstance();

			builder.RegisterType<ASKContainer>()
				.As<ASPSecurityKit.IContainer>()
				.InstancePerLifetimeScope();

		}
	}
}
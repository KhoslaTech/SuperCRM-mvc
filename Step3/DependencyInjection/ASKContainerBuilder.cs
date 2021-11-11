using System;
using ASPSecurityKit;
using Autofac;

namespace SuperCRM.DependencyInjection
{
	public class ASKContainerBuilder : IContainerBuilder
	{
		private readonly ContainerBuilder containerBuilder;

		public ASKContainerBuilder(ContainerBuilder containerBuilder)
		{
			this.containerBuilder = containerBuilder;
		}

		public void Register(Type concrete, InstanceScope scope)
		{
			switch (scope)
			{
				case InstanceScope.Request:
					this.containerBuilder.RegisterType(concrete).AsSelf().InstancePerLifetimeScope();
					break;
				case InstanceScope.Singleton:
					this.containerBuilder.RegisterType(concrete).AsSelf().SingleInstance();
					break;
				case InstanceScope.Transient:
					this.containerBuilder.RegisterType(concrete).AsSelf().InstancePerDependency();
					break;
				default:
					throw new NotSupportedException($"{concrete}:{scope} scope not supported.");
			}
		}

		public void Register<TConcrete>(InstanceScope scope) where TConcrete : class
		{
			switch (scope)
			{
				case InstanceScope.Request:
					this.containerBuilder.RegisterType<TConcrete>().AsSelf().InstancePerLifetimeScope();
					break;
				case InstanceScope.Singleton:
					this.containerBuilder.RegisterType<TConcrete>().AsSelf().SingleInstance();
					break;
				case InstanceScope.Transient:
					this.containerBuilder.RegisterType<TConcrete>().AsSelf().InstancePerDependency();
					break;
				default:
					throw new NotSupportedException($"{typeof(TConcrete)}:{scope} scope not supported.");
			}
		}

		public void Register<TConcrete>(Type[] contracts, InstanceScope scope) where TConcrete : class
		{
			switch (scope)
			{
				case InstanceScope.Request:
					this.containerBuilder.RegisterType<TConcrete>().As(contracts).InstancePerLifetimeScope();
					break;
				case InstanceScope.Singleton:
					this.containerBuilder.RegisterType<TConcrete>().As(contracts).SingleInstance();
					break;
				case InstanceScope.Transient:
					this.containerBuilder.RegisterType<TConcrete>().As(contracts).InstancePerDependency();
					break;
				default:
					throw new NotSupportedException($"{typeof(TConcrete)}:{scope} scope not supported.");
			}
		}

		public void Register<TContract, TConcrete>(InstanceScope scope) where TConcrete : class, TContract
		{
			switch (scope)
			{
				case InstanceScope.Request:
					this.containerBuilder.RegisterType<TConcrete>().As<TContract>().InstancePerLifetimeScope();
					break;
				case InstanceScope.Singleton:
					this.containerBuilder.RegisterType<TConcrete>().As<TContract>().SingleInstance();
					break;
				case InstanceScope.Transient:
					this.containerBuilder.RegisterType<TConcrete>().As<TContract>().InstancePerDependency();
					break;
				default:
					throw new NotSupportedException($"{typeof(TConcrete)}-{typeof(TContract)}:{scope} scope not supported.");
			}
		}

		public void Register<TContract>(Func<ASPSecurityKit.IContainer, TContract> factory, InstanceScope scope) where TContract : class
		{
			switch (scope)
			{
				case InstanceScope.Request:
					this.containerBuilder.Register(context => factory(new ASKResolver(context))).As<TContract>().InstancePerLifetimeScope();
					break;
				case InstanceScope.Singleton:
					this.containerBuilder.Register(context => factory(new ASKResolver(context))).As<TContract>().SingleInstance();
					break;
				case InstanceScope.Transient:
					this.containerBuilder.Register(context => factory(new ASKResolver(context))).As<TContract>().InstancePerDependency();
					break;
				default:
					throw new NotSupportedException($"{typeof(TContract)}:{scope} scope not supported.");
			}
		}
	}
}
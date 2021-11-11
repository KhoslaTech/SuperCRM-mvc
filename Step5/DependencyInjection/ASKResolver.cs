using System;
using Autofac;
using IContainer = ASPSecurityKit.IContainer;

namespace SuperCRM.DependencyInjection
{
	public class ASKResolver : IContainer
	{
		private readonly IComponentContext context;

		public ASKResolver(IComponentContext context)
		{
			this.context = context;
		}

		public TPlugin Resolve<TPlugin>()
		{
			return this.context.Resolve<TPlugin>();
		}

		public object Resolve(Type pluginType)
		{
			return this.context.Resolve(pluginType);
		}
	}
}
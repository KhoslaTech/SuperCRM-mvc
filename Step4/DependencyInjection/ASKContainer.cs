using System;
using ASPSecurityKit;
using Microsoft.Extensions.DependencyInjection;

namespace SuperCRM.DependencyInjection
{
	public class ASKContainer : IContainer
	{
		private readonly IServiceProvider serviceProvider;

		public ASKContainer(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public TPlugin Resolve<TPlugin>()
		{
			return this.serviceProvider.GetService<TPlugin>();
		}

		public object Resolve(Type pluginType)
		{
			return this.serviceProvider.GetService(pluginType);
		}
	}
}
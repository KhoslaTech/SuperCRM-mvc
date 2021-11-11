using System;
using ASPSecurityKit;
using Microsoft.Extensions.Configuration;

namespace SuperCRM.Security
{
	public class Config : IConfig
	{
		private readonly IConfiguration configuration;

		public Config(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public T GetSettingOrDefault<T>(string key, T defaultValue = default)
		{
			try
			{
				var value = this.configuration[key];

				if (typeof(T).IsEnum)
					return (T)Enum.Parse(typeof(T), value, true);

				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}
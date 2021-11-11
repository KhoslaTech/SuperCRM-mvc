using System;
using System.Threading;
using System.Threading.Tasks;
using ASPSecurityKit;

namespace SuperCRM.Security
{
	public class Logger : ILogger
	{
		public void Error(Exception ex)
		{
			System.Diagnostics.Trace.TraceError($"error: {ex.Message}\r\ntrace:{ex}");
		}

		public Task ErrorAsync(Exception ex)
		{
			return ErrorAsync(ex, CancellationToken.None);
		}

		public async Task ErrorAsync(Exception ex, CancellationToken cancellationToken)
		{
			await Task.Run(() => { System.Diagnostics.Trace.TraceError($"error: {ex.Message}\r\ntrace:{ex}"); },
				cancellationToken).ConfigureAwait(false);
		}

		public void Error(Exception ex, string msg, params object[] args)
		{
			Error(new AnitatedException(string.Format(msg, args), ex));
		}

		public Task ErrorAsync(Exception ex, string msg, params object[] args)
		{
			return ErrorAsync(ex, msg, CancellationToken.None, args);
		}

		public Task ErrorAsync(Exception ex, string msg, CancellationToken cancellationToken, params object[] args)
		{
			return ErrorAsync(new AnitatedException(string.Format(msg, args), ex), cancellationToken);
		}

		public void Info(string msg, params object[] args)
		{
			System.Diagnostics.Trace.TraceInformation(msg, args);
		}

		public Task InfoAsync(string msg, params object[] args)
		{
			return InfoAsync(msg, CancellationToken.None, args);
		}

		public async Task InfoAsync(string msg, CancellationToken cancellationToken, params object[] args)
		{
			await Task.Run(() => { System.Diagnostics.Trace.TraceInformation(msg, args); },
				cancellationToken).ConfigureAwait(false);
		}

		public void Warn(string msg, params object[] args)
		{
			System.Diagnostics.Trace.TraceWarning(msg, args);
		}

		public Task WarnAsync(string msg, params object[] args)
		{
			return WarnAsync(msg, CancellationToken.None, args);
		}

		public async Task WarnAsync(string msg, CancellationToken cancellationToken, params object[] args)
		{
			await Task.Run(() => { System.Diagnostics.Trace.TraceWarning(msg, args); },
				cancellationToken).ConfigureAwait(false);
		}

		class AnitatedException : Exception
		{
			public AnitatedException(string message, Exception innerException)
				: base(message, innerException)
			{ }
		}
	}
}
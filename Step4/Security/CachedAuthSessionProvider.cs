using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.AuthProviders;

namespace SuperCRM.Security
{
	public class CachedAuthSessionProvider : AuthSessionProvider
	{
		private readonly ICacheClient cacheClient;

		public CachedAuthSessionProvider(IUserService userService,
			ISecuritySettings settings, IIdentityRepository identityRepository, IBrowser browser,
			ISecurityContext securityContext, ISessionService sessionService, ISecurityUtility securityUtility,
			IAuthCookieProvider authCookieProvider, IErrorMessageResourceProvider errorResource, ICacheClient cacheClient) :
			base(userService, settings, identityRepository, browser, securityContext, sessionService, securityUtility, authCookieProvider, errorResource)
		{
			this.cacheClient = cacheClient;
		}

		public override void LoadSession(IAuthDetails auth)
		{
			this.SessionService.Load(this.cacheClient.Get<Dictionary<string, object>>(auth.AuthUrn) ?? new Dictionary<string, object>(), auth.AuthUrn);
		}

		public override Task LoadSessionAsync(IAuthDetails auth)
		{
			return this.LoadSessionAsync(auth, CancellationToken.None);
		}

		public override async Task LoadSessionAsync(IAuthDetails auth, CancellationToken cancellationToken)
		{
			this.SessionService.Load((await this.cacheClient.GetAsync<Dictionary<string, object>>(auth.AuthUrn, cancellationToken).ConfigureAwait(false)) ?? new Dictionary<string, object>(), auth.AuthUrn);
		}

		public void SaveSession()
		{
			this.cacheClient.Set(this.SessionService.SessionID, this.SessionService.GetSessionState());
		}

		public Task SaveSessionAsync()
		{
			return this.SaveSessionAsync(CancellationToken.None);
		}

		public async Task SaveSessionAsync(CancellationToken cancellationToken)
		{
			await this.cacheClient.SetAsync(this.SessionService.SessionID, this.SessionService.GetSessionState(), cancellationToken).ConfigureAwait(false);
		}
	}
}
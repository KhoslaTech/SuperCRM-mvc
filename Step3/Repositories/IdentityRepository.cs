using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SuperCRM.DataModels;
using SuperCRM.Infrastructure;
using SuperCRM.Models;
using SuperCRM.Security;
using ASPSecurityKit;
using ASPSecurityKit.AuthProviders;
using Microsoft.EntityFrameworkCore;

namespace SuperCRM.Repositories
{
	public class IdentityRepository : IIdentityRepository
	{
		private readonly AppDbContext dbContext;
		private readonly IBrowser browser;
		private readonly ISecuritySettings securitySettings;
		private readonly ISecurityContext securityContext;
		private readonly ISecurityUtility securityUtility;
		private readonly ILogger logger;

		public IdentityRepository(AppDbContext dbContext, IBrowser browser,
			ISecuritySettings securitySettings, ISecurityContext securityContext,
			ISecurityUtility securityUtility, ILogger logger)
		{
			this.dbContext = dbContext;
			this.browser = browser;
			this.securitySettings = securitySettings;
			this.securityContext = securityContext;
			this.securityUtility = securityUtility;
			this.logger = logger;
		}

		public IAuthDetails GetAuth(string authUrn)
		{
			var parts = authUrn.Split(':');
			if (parts.Length != 2)
			{
				return null;
			}

			var authType = parts[0].ToLower();
			var authKey = new Guid(parts[1]);

			switch (authType)
			{
				case "sessionid":
					var dbSession = dbContext.UserSessions.Where(x => x.Id == authKey).Include(m => m.User.PermitGroups).SingleOrDefault();

					if (dbSession != null)
					{
						var auth = new IdentityAuthDetails
						{
							Secret = dbSession.Secret,
							AuthUrn = IdentityTokenType.MakeSession(dbSession.Id.ToString("N")),
							UserId = dbSession.User.Id,
							EffectiveFrom = dbSession.EffectiveFrom,
							ExpiredOn = dbSession.ExpiredOn,
							ImpersonatedUserId = dbSession.ImpersonatedUserId,
							SlidingExpiration = dbSession.SlidingExpiration,
							SlideByDurationInMinutes = dbSession.SlideByDurationInMinutes,
					};
						auth.FirewallIpRanges = FirewallIpRange.RangeForWholeOfInternet();
						AddLocalhostIpToFirewall(auth);
						return auth;
					}

					break;
				default:
					throw new NotSupportedException($"{authType} not supported.");
			}

			return null;
		}

		public Task<IAuthDetails> GetAuthAsync(string authUrn)
		{
			return this.GetAuthAsync(authUrn, CancellationToken.None);
		}

		public async Task<IAuthDetails> GetAuthAsync(string authUrn, CancellationToken cancellationToken)
		{
			var parts = authUrn.Split(':');
			if (parts.Length != 2)
			{
				return null;
			}

			var authType = parts[0].ToLower();
			var authKey = new Guid(parts[1]);

			switch (authType)
			{
				case "sessionid":
					var dbSession = await dbContext.UserSessions.Where(x => x.Id == authKey)
						.Include(m => m.User.PermitGroups)
						.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

					if (dbSession != null)
					{
						var auth = new IdentityAuthDetails
						{
							Secret = dbSession.Secret,
							AuthUrn = IdentityTokenType.MakeSession(dbSession.Id.ToString("N")),
							UserId = dbSession.User.Id,
							EffectiveFrom = dbSession.EffectiveFrom,
							ExpiredOn = dbSession.ExpiredOn,
							ImpersonatedUserId = dbSession.ImpersonatedUserId,
							SlidingExpiration = dbSession.SlidingExpiration,
							SlideByDurationInMinutes = dbSession.SlideByDurationInMinutes,
						};
						auth.FirewallIpRanges = FirewallIpRange.RangeForWholeOfInternet();
						AddLocalhostIpToFirewall(auth);
						return auth;
					}

					break;
				default:
					throw new NotSupportedException($"{authType} not supported.");
			}

			return null;
		}

		public IAuthDetails CreateNewUserSession(IUser user, bool longLived, Guid? impersonatedUserId = null)
		{
			if (!(user is DbUser dbUser))
				throw new InvalidOperationException($"Unexpected: {user} is not of type {typeof(DbUser)}.");

			var userSession = new DbUserSession
			{
				Id = Guid.NewGuid(),
				UserId = dbUser.Id,
				Secret = this.securityUtility.NewSecret(),
				EffectiveFrom = DateTime.UtcNow,
				ExpiredOn = DateTime.UtcNow.AddMinutes(longLived ? securitySettings.RememberMeTimeoutInMinutes : 30),
				SlidingExpiration = true,
				SlideByDurationInMinutes = longLived ? securitySettings.RememberMeTimeoutInMinutes : 30,
				ImpersonatedUserId = impersonatedUserId,
				Ip = securityContext.RequestService.GetCallerIp(),
				Device = browser.GetIdentifier(securityContext.RequestService.UserAgent)
			};

			dbContext.UserSessions.Add(userSession);
			dbContext.SaveChanges();

			return GetAuth(IdentityTokenType.MakeSession(userSession.Id.ToString("N")));
		}

		public Task<IAuthDetails> CreateNewUserSessionAsync(IUser user, bool longLived, Guid? impersonatedUserId = null)
		{
			return this.CreateNewUserSessionAsync(user, longLived, CancellationToken.None, impersonatedUserId);
		}

		public async Task<IAuthDetails> CreateNewUserSessionAsync(IUser user, bool longLived, CancellationToken cancellationToken,
			Guid? impersonatedUserId = null)
		{
			if (!(user is DbUser dbUser))
				throw new InvalidOperationException($"Unexpected: {user} is not of type {typeof(DbUser)}.");

			var userSession = new DbUserSession
			{
				Id = Guid.NewGuid(),
				UserId = dbUser.Id,
				Secret = this.securityUtility.NewSecret(),
				EffectiveFrom = DateTime.UtcNow,
				ExpiredOn = DateTime.UtcNow.AddMinutes(longLived ? securitySettings.RememberMeTimeoutInMinutes : 30),
				SlidingExpiration = true,
				SlideByDurationInMinutes = longLived ? securitySettings.RememberMeTimeoutInMinutes : 30,
				ImpersonatedUserId = impersonatedUserId,
				Ip = await securityContext.RequestService.GetCallerIpAsync(cancellationToken).ConfigureAwait(false),
				Device = browser.GetIdentifier(securityContext.RequestService.UserAgent)
			};

			dbContext.UserSessions.Add(userSession);
			await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return await GetAuthAsync(IdentityTokenType.MakeSession(userSession.Id.ToString("N")), cancellationToken).ConfigureAwait(false);
		}

		public bool SlideExpiration(IAuthDetails auth)
		{
			// No need to update sliding expiration on every request – for long-remembered session we can do that once every hour. For rest can update per minute.
			if (IdentityTokenType.IsSession(auth.AuthUrn) &&
					 auth.SlidingExpiration && !this.securityUtility.IsSlidRecently(auth.ExpiredOn, auth.SlideByDurationInMinutes.Value,
							 auth.SlideByDurationInMinutes == this.securitySettings.RememberMeTimeoutInMinutes ? 60 : 1))
			{
				var sessionId = Guid.Parse(IdentityTokenType.GetToken(auth.AuthUrn));

				var dbSession = dbContext.UserSessions.SingleOrDefault(x => x.Id == sessionId);
				if (dbSession == null)
				{
					logger.Warn($"Warning: {auth.AuthUrn} not found.");
					return false;
				}

				dbSession.ExpiredOn = DateTime.UtcNow.AddMinutes(auth.SlideByDurationInMinutes.Value);
				auth.ExpiredOn = dbSession.ExpiredOn;

				dbContext.SaveChanges();

				return true;
			}

			return false;
		}

		public Task<bool> SlideExpirationAsync(IAuthDetails auth)
		{
			return this.SlideExpirationAsync(auth, CancellationToken.None);
		}

		public async Task<bool> SlideExpirationAsync(IAuthDetails auth, CancellationToken cancellationToken)
		{
			// No need to update sliding expiration on every request – for long-remembered session we can do that once every hour. For rest can update per minute.
			if (IdentityTokenType.IsSession(auth.AuthUrn) &&
					 auth.SlidingExpiration && !this.securityUtility.IsSlidRecently(auth.ExpiredOn, auth.SlideByDurationInMinutes.Value,
						 auth.SlideByDurationInMinutes == this.securitySettings.RememberMeTimeoutInMinutes ? 60 : 1))
			{
				var sessionId = Guid.Parse(IdentityTokenType.GetToken(auth.AuthUrn));

				var dbSession = await dbContext.UserSessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken).ConfigureAwait(false);
				if (dbSession == null)
				{
					await logger.WarnAsync($"Warning: {auth.AuthUrn} not found.", cancellationToken).ConfigureAwait(false);
					return false;
				}

				dbSession.ExpiredOn = DateTime.UtcNow.AddMinutes(auth.SlideByDurationInMinutes.Value);
				auth.ExpiredOn = dbSession.ExpiredOn;

				await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

				return true;
			}

			return false;
		}

		public void SlideRecentAccessWithMFAVerification(string authUrn)
		{
			throw new NotImplementedException();
		}

		public Task SlideRecentAccessWithMFAVerificationAsync(string authUrn)
		{
			return this.SlideRecentAccessWithMFAVerificationAsync(authUrn, CancellationToken.None);
		}

#pragma warning disable 1998
		public async Task SlideRecentAccessWithMFAVerificationAsync(string authUrn, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
#pragma warning restore 1998

		public void SetMFAVerificationValidUntilSessionExpired(string authUrn)
		{
			throw new NotImplementedException();
		}

		public Task SetMFAVerificationValidUntilSessionExpiredAsync(string authUrn)
		{
			return this.SetMFAVerificationValidUntilSessionExpiredAsync(authUrn, CancellationToken.None);
		}

#pragma warning disable 1998
		public async Task SetMFAVerificationValidUntilSessionExpiredAsync(string authUrn, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
#pragma warning restore 1998

		public void Expire(string authUrn)
		{
			if (IdentityTokenType.IsSession(authUrn))
			{
				var sessionId = Guid.Parse(IdentityTokenType.GetToken(authUrn));

				var dbUserSession = dbContext.UserSessions.SingleOrDefault(x => x.Id == sessionId);
				if (dbUserSession != null)
				{
					dbUserSession.ExpiredOn = DateTime.UtcNow.AddMinutes(-1); // To handle slight variations in time within a scaled out servers cluster
					dbContext.SaveChanges();
				}
				else
				{
					logger.Warn($"Warning: {authUrn} not found.");
				}
			}
		}

		public Task ExpireAsync(string authUrn)
		{
			return this.ExpireAsync(authUrn, CancellationToken.None);
		}

		public async Task ExpireAsync(string authUrn, CancellationToken cancellationToken)
		{
			if (IdentityTokenType.IsSession(authUrn))
			{
				var sessionId = Guid.Parse(IdentityTokenType.GetToken(authUrn));

				var dbUserSession = await dbContext.UserSessions
					.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken)
					.ConfigureAwait(false);

				if (dbUserSession != null)
				{
					dbUserSession.ExpiredOn = DateTime.UtcNow.AddMinutes(-1); // To handle slight variations in time within a scaled out servers cluster
					await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
				}
				else
				{
					await logger.WarnAsync($"Warning: {authUrn} not found.", cancellationToken).ConfigureAwait(false);
				}
			}
		}

		public void ExpireUserSessions(IUser user, string exceptThisSessionUrn = null)
		{
			var dbUser = user as DbUser;
			var sessionId = !string.IsNullOrWhiteSpace(exceptThisSessionUrn)
				? Guid.Parse(IdentityTokenType.GetToken(exceptThisSessionUrn))
				: (Guid?)null;

			var userSessions = this.dbContext.UserSessions.Where(x =>
				x.UserId == dbUser.Id && x.ExpiredOn > DateTime.UtcNow && x.Id != sessionId);

			foreach (var session in userSessions)
			{
				session.ExpiredOn = DateTime.UtcNow.AddMinutes(-10);
			}

			this.dbContext.SaveChanges();
		}

		public Task ExpireUserSessionsAsync(IUser user, string exceptThisSessionUrn = null)
		{
			return ExpireUserSessionsAsync(user, CancellationToken.None, exceptThisSessionUrn);
		}

		public async Task ExpireUserSessionsAsync(IUser user, CancellationToken cancellationToken, string exceptThisSessionUrn = null)
		{
			var dbUser = user as DbUser;
			var sessionId = !string.IsNullOrWhiteSpace(exceptThisSessionUrn)
				? Guid.Parse(IdentityTokenType.GetToken(exceptThisSessionUrn))
				: (Guid?)null;

			var userSessions = this.dbContext.UserSessions.Where(x =>
				x.UserId == dbUser.Id && x.ExpiredOn > DateTime.UtcNow && x.Id != sessionId);

			foreach (var session in userSessions)
			{
				session.ExpiredOn = DateTime.UtcNow.AddMinutes(-10);
			}

			await this.dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private IAuthDetails AddLocalhostIpToFirewall(IAuthDetails auth)
		{
			if (ASPSecurityKitConfiguration.IsDevelopmentEnvironment)
			{
				if (auth.FirewallIpRanges == null)
				{
					auth.FirewallIpRanges = FirewallIpRange.RangeForLocalhost();
				}
				else
				{
					auth.FirewallIpRanges.AddRange(FirewallIpRange.RangeForLocalhost());
				}
			}

			return auth;
		}
	}
}
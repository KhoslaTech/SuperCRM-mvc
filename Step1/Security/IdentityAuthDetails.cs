using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.AuthProviders;

namespace SuperCRM.Security
{
	public class IdentityAuthDetails : IAuthDetails<Guid>
	{
		public string AuthUrn { get; set; }
		public string Secret { get; set; }
		public DateTime EffectiveFrom { get; set; }
		public DateTime? ExpiredOn { get; set; }
		public bool SlidingExpiration { get; set; }
		public int? SlideByDurationInMinutes { get; set; }
		public bool KeyBasedAuthAllowed { get; set; }
		public DateTime? RecentAccessWithMFAAt { get; set; }
		public bool? MFAValidUntilSessionExpired { get; set; }
		public bool MFAEnforced { get; set; }
		public KeyType? KeyType { get; set; }
		public List<IFirewallIpRange> FirewallIpRanges { get; set; }
		public List<IFirewallIpRange> MFAWhiteListedIpRanges { get; set; }
		public List<IWhitelistedDomain> OriginDomains { get; set; }
		public Guid? UserId { get; set; }
		public Guid? PermitGroupId { get; set; }
		public Guid? ImpersonatedUserId { get; set; }

		public bool IsMFASupported()
		{
			return false;
		}

		public Task<bool> IsMFASupportedAsync()
		{
			return this.IsMFASupportedAsync(CancellationToken.None);
		}

		public async Task<bool> IsMFASupportedAsync(CancellationToken cancellationToken)
		{
			return await Task.FromResult(false).ConfigureAwait(false);
		}
	}
}
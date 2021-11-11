using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperCRM.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperCRM.Security
{
	// Note: the starter/premium source packages have a better implementation of error handling with error pages and json responses using exception middlewear and error controller https://ASPSecurityKit.net/docs/source-packages/?packageId=premium-netCoreMvc#error-pages
	public class AjaxSecurityFailureResponseHandler : MvcSecurityFailureResponseHandler
	{
		private readonly INetSecuritySettings securitySettings;
		private readonly ISecurityUtility securityUtility;

		public AjaxSecurityFailureResponseHandler(INetSecuritySettings securitySettings, IBrowser browser, ISecurityUtility securityUtility) : base(securitySettings, browser)
		{
			this.securitySettings = securitySettings;
			this.securityUtility = securityUtility;
		}

		public override async Task<bool> HandleFailureAsync(INetRequestService requestService, OpResult reason, string failureDescription, List<AuthError> errors, CancellationToken cancellationToken)
		{
			if (requestService.IsApiRequest())
			{
				if (errors?.Count > 0)
				{
					failureDescription += "\r\n" +  string.Join("\r\n", errors.Select(x => x.ErrorDetail));
				}

				// writing 200 instead of getting the right code from this.securityUtility.OpResultToStatusCode(reason) because jtable only shows error in case of 200. Otherwise, it just shows the generic error message.
				await requestService.WriteToResponseAsync(ApiResponse.Error(this.securityUtility.GetErrorMessage(reason, failureDescription, this.securitySettings.IsDevelopmentEnvironment)), cancellationToken, 200);
				return true;
			}

			return await base.HandleFailureAsync(requestService, reason, failureDescription, errors, cancellationToken);
		}

	}
}

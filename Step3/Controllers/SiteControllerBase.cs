using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperCRM.DataModels;
using SuperCRM.Infrastructure;
using SuperCRM.Models;
using SuperCRM.Security;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace SuperCRM.Controllers
{
	[AcceptAuthCookie]
	public abstract class SiteControllerBase : Controller
	{
		protected const string VD_ActionResponse = "ActionResponse";

		#region Message Helpers

		protected void SetRedirectMessage(string message, OpResult result)
		{
			TempData[VD_ActionResponse] = JsonConvert.SerializeObject(new ActionResponse { Result = result, Message = message });
		}

		protected ActionResult RedirectWithMessage(string actionName, string controllerName, object routeValues, string message, OpResult result)
		{
			SetRedirectMessage(message, result);
			return RedirectToAction(actionName, controllerName, routeValues);
		}

		protected ActionResult RedirectWithMessage(string actionName, string controllerName, string message, OpResult result)
		{
			SetRedirectMessage(message, result);
			return RedirectToAction(actionName, controllerName);
		}

		protected ActionResult RedirectWithMessage(string actionName, string message, OpResult result)
		{
			SetRedirectMessage(message, result);
			return RedirectToAction(actionName);
		}

		protected ActionResult RedirectToIndexWithMessage(string message, OpResult result)
		{
			return RedirectWithMessage("Index", message, result);
		}

		protected ActionResult RedirectToIndexWithMessage(string controllerName, string message, OpResult result, object routeValues = null)
		{
			return RedirectWithMessage("Index", controllerName, routeValues, message, result);
		}

		/// <summary>
		/// Sets a message that will be displayed on this page load.
		/// </summary>
		protected void SetMessage(string message, OpResult result)
		{
			ViewData[VD_ActionResponse] = new ActionResponse { Result = result, Message = message };
		}

		#endregion

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// pull the message (if any) left by prior page into view data to render it as part of this page.
			if (TempData.TryGetValue(VD_ActionResponse, out var value))
			{
				ViewData[VD_ActionResponse] = JsonConvert.DeserializeObject<ActionResponse>((string)value);
			}
		}
	}

	public abstract class ServiceControllerBase : SiteControllerBase
	{
		protected readonly IUserService<Guid, Guid, DbUser> UserService;
		protected readonly INetSecuritySettings SecuritySettings;
		protected readonly ISecurityUtility SecurityUtility;
		protected readonly ILogger Logger;

		protected ServiceControllerBase(IUserService<Guid, Guid, DbUser> userService, INetSecuritySettings securitySettings, 
			ISecurityUtility securityUtility, ILogger logger)
		{
			this.UserService = userService;
			this.SecuritySettings = securitySettings;
			this.SecurityUtility = securityUtility;
			this.Logger = logger;
		}

		#region Json Methods
		/// <summary>
		/// A wrapper for json requests.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		protected virtual async Task<ActionResult> SecureJsonAction(Func<Task<ActionResult>> method)
		{
			try
			{
				return await method();
			}
			catch (DbUpdateException efEx)
			{
				var ex = efEx.GetBaseException() as SqlException;
				if (ex != null && (ex.Number == (int)SqlErrors.KeyViolation || ex.Number == (int)SqlErrors.UniqueIndex))
				{
					return Json(ApiResponse.Error("Cannot add a duplicate record"));
				}
				
				await this.Logger.ErrorAsync(ex).ConfigureAwait(false);
				return Json(ApiResponse.Error(this.SecurityUtility.GetErrorMessage(OpResult.DBError, ex.Message, this.SecuritySettings.IsDevelopmentEnvironment)));
			}
			catch (OpException ex)
			{
				switch (ex.Reason)
				{
					case OpResult.InvalidInput:
						var result = await GetResult(ex.IsErrorMessageProvided ? ex.Message : null);
						return Json(ApiResponse.Error(result.Message));
					default:
						return Json(ApiResponse.Error(this.SecurityUtility.GetErrorMessage(ex.Reason, $"{ex.Reason}:{ex.Message}", this.SecuritySettings.IsDevelopmentEnvironment)));
				}
			}
			catch (Exception ex)
			{
				await this.Logger.ErrorAsync(ex).ConfigureAwait(false);
				return Json(ApiResponse.Error(this.SecurityUtility.GetErrorMessage(OpResult.SomeError, ex.Message, this.SecuritySettings.IsDevelopmentEnvironment)));
			}
		}

		/// <summary>
		/// Returns <see cref="ActionResponse"/> instance either from the ViewData or by creating a new instance from ModelState errors.
		/// </summary>
		protected virtual async Task<ActionResponse> GetResult(string errorMessage)
		{
			var response = ViewData[VD_ActionResponse] as ActionResponse;
			// ViewData[ActionResponse] and ModelState are mutually exclusive. For this reason, you must not use both in the same method.
			// ActionResponse is given preference over ModelState –  so if the former is present, latter will not be considered.
			if (response == null)
			{
				var errors = new List<string>();

				foreach (var state in ModelState.Values)
				{
					foreach (var err in state.Errors)
						errors.Add(err.ErrorMessage);
				}

				if (errors.Count == 0)
				{
					if (!string.IsNullOrWhiteSpace(errorMessage))
						response = new ActionResponse { Result = OpResult.InvalidInput, Message = errorMessage };
					else
					// unexpected: no response information could be inferred –  log and show default error message.
					{
						await this.Logger.WarnAsync("Unexpected: missing response to be returned: returning generic.");
						response = new ActionResponse { Result = OpResult.SomeError, Message = "Unexpected error" };
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(errorMessage))
						errors.Insert(0, errorMessage);

					response = new ActionResponse { Result = OpResult.InvalidInput, Message = ToHTML(errors) };
				}
			}
			else if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				response.Message += $"\r\n{errorMessage}";
			}

			return response;
		}

		private static string ToHTML(List<string> list)
		{
			var text = new StringBuilder();
			text.Append("<ul>");
			foreach (var li in list)
				text.Append($"<li>{li}</li>");

			text.Append("</ul>");
			return text.ToString();
		}
		#endregion

	}
}
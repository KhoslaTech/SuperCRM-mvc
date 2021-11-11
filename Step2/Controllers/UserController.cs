using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperCRM.DataModels;
using SuperCRM.Models;

namespace SuperCRM.Controllers
{
	public class UserController : ServiceControllerBase
	{
		private readonly IAuthSessionProvider authSessionProvider;

		public UserController(IUserService<Guid, Guid, DbUser> userService, INetSecuritySettings securitySettings,
			ISecurityUtility securityUtility, ILogger logger, IAuthSessionProvider authSessionProvider) : base(userService, securitySettings, securityUtility,
			logger)
		{
			this.authSessionProvider = authSessionProvider;
		}

		[AllowAnonymous]
		public ActionResult SignUp()
		{
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> SignUp(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				var dbUser = await this.UserService.NewUserAsync(model.Email, model.Password, model.Name);

				dbUser.Id = Guid.NewGuid();
				if (model.Type == AccountType.Team)
					dbUser.BusinessDetails = new DbBusinessDetails { Name = model.BusinessName };

				if (await this.UserService.CreateAccountAsync(dbUser))
				{
					await this.authSessionProvider.LoginAsync(model.Email, model.Password, false, this.SecuritySettings.LetSuspendedAuthenticate, true);

					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "An account with this email is already registered.");
				}

			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[AllowAnonymous]
		public ActionResult SignIn(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> SignIn(LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var result = await this.authSessionProvider.LoginAsync(model.Email, model.Password, model.RememberMe, this.SecuritySettings.LetSuspendedAuthenticate, true);
				switch (result.Result)
				{
					case OpResult.Success:
						// we should never redirect the user to sign-out automatically
						if (Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("user/signout", StringComparison.InvariantCultureIgnoreCase))
						{
							return Redirect(returnUrl);
						}

						return RedirectToAction("Index", "Home");
					case OpResult.Suspended:
						ModelState.AddModelError(string.Empty, "This account is suspended.");
						break;
					case OpResult.PasswordBlocked:
						ModelState.AddModelError(string.Empty, "Your password is blocked. Please reset the password using the 'forgot password' option.");
						break;
					default:
						ModelState.AddModelError(string.Empty, "The email or password provided is incorrect.");
						break;
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[Feature(RequestFeature.AuthorizationNotRequired, RequestFeature.MFANotRequired)]
		public async Task<ActionResult> SignOut()
		{
			await this.authSessionProvider.LogoutAsync();
			return RedirectToAction("SignIn", "User");
		}
	}
}

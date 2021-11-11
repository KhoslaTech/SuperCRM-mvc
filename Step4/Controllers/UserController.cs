using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperCRM.DataModels;
using SuperCRM.Models;
using SuperCRM.Repositories;

namespace SuperCRM.Controllers
{
	public class UserController : ServiceControllerBase
	{
		private readonly IAuthSessionProvider authSessionProvider;
		private readonly IUserPermitRepository permitRepository;

		public UserController(IUserService<Guid, Guid, DbUser> userService, INetSecuritySettings securitySettings,
			ISecurityUtility securityUtility, ILogger logger, IAuthSessionProvider authSessionProvider) : base(userService, securitySettings, securityUtility,
			logger)
		{
			this.authSessionProvider = authSessionProvider;
			this.permitRepository = permitRepository;
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
					await this.permitRepository.AddPermitAsync(dbUser.Id, "Customer", dbUser.Id);
					await SendVerificationMailAsync(dbUser);
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

		private async Task SendVerificationMailAsync(DbUser user)
		{
			// to use Gmail, you need to enable "Less secure app access" etc. for more information, visit https://support.google.com/a/answer/176600?hl=en#zippy=%2Cuse-the-restricted-gmail-smtp-server%2Cuse-the-gmail-smtp-server
			var username = "<YourGMailSmtpUsername>";
			var password = "<YourGMailSmtpPassword>";
			var host = "smtp.gmail.com";
			var verificationUrl = Url.Action("Verification", "User", new { id = user.VerificationToken }, Request.Scheme);

			var mail = new MailMessage { From = new MailAddress(username) };
			mail.To.Add(user.Username);
			mail.Subject = "Verify your email";
			mail.Body = $@"<p>Hi {user.Name},<br/>Please click the link below to verify your email.<br/><a href='{verificationUrl}'>{verificationUrl}</a><br/>Thank you!</p>";
			mail.IsBodyHtml = true;

			var smtp = new SmtpClient(host, 587)
			{
				Credentials = new NetworkCredential(username, password),
				EnableSsl = true
			};

			await smtp.SendMailAsync(mail).ConfigureAwait(false);
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

		[VerificationNotRequired, SkipActivityAuthorization]
		public ActionResult Verify()
		{
			return View();
		}

		[HttpPost, ValidateAntiForgeryToken]
		[VerificationNotRequired, SkipActivityAuthorization]
		public async Task<ActionResult> ResendVerificationEmail()
		{
			if (this.UserService.IsAuthenticated && this.UserService.IsVerified)
			{
				return RedirectWithMessage("Index", "Home", "Account already verified", OpResult.Success);
			}

			await SendVerificationMailAsync(this.UserService.CurrentUser);
			return RedirectWithMessage("Verify",
				$"We have sent you a mail at {this.UserService.CurrentUsername} – please make sure you follow the link in the mail and verify your email. Please check your email now. It may take a while to reach your inbox.", OpResult.Success);
		}

		[AllowAnonymous]
		public async Task<ActionResult> Verification(string id)
		{
			switch (await this.UserService.VerifyAccountAsync(id))
			{
				case OpResult.Success:
					return RedirectWithMessage("Index", "Home", "Congratulation! Your account has been successfully verified!.", OpResult.Success);
				case OpResult.AlreadyDone:
					return RedirectWithMessage("Index", "Home", "Account already verified", OpResult.Success);
				default:
					return RedirectWithMessage("Verify", "Verification was not successful; please try again.", OpResult.SomeError);
			}
		}
	}
}

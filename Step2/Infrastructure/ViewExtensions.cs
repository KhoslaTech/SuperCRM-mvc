using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ASPSecurityKit;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SuperCRM.Models;

namespace SuperCRM.Infrastructure
{
	public static class ViewExtensions
	{
		// for better popup-based error provider instead, use starter or higher source package https://aspsecuritykit.net/docs/source-packages/?packageId=premium-netCoreMvc#error-pages
		public static HtmlString ErrorProvider(this IHtmlHelper html)
		{
			var dictionary = new Microsoft.AspNetCore.Routing.RouteValueDictionary();
			dictionary.Add("data-valmsg-summary", false);
			return ErrorProvider(html, dictionary);
		}

		private static HtmlString ErrorProvider(IHtmlHelper html, IDictionary<string, object> attr)
		{
			var response = html.ViewContext.ViewData[Constants.VD_ActionResponse] as ActionResponse;

			// ViewData[Response] and ModelState are mutually exclusive. For this reason, you must not use both in the same method.
			// ActionResponse is given preference over ModelState –  so if the former is present, latter will not be considered.

			if (response == null)
			{
				List<string> errors = new List<string>();

				foreach (var modelState in html.ViewData.ModelState)
				{
					if (string.IsNullOrWhiteSpace(modelState.Key))
					{
						foreach (var err in modelState.Value.Errors)
							errors.Add(err.ErrorMessage);
					}
				}

				if (errors.Count > 0)
					response = new ActionResponse { Result = OpResult.InvalidInput, Message = errors.ToHTML() };
			}

			var additionalAttrs = string.Empty;
			if (attr != null)
			{
				additionalAttrs = " " + string.Join(" ",
					attr.Select(it =>
						$"{it.Key}=\"{(it.Value is bool ? it.Value.ToString().ToLower() : it.Value.ToString())}\""));
			}

			if (response == null
				|| string.IsNullOrWhiteSpace(response.Message))
			{
				return new HtmlString(
					$"<div id='errorProvider' class='{HtmlHelper.ValidationSummaryValidCssClassName}'{additionalAttrs}></div>");
			}

			if (response.Result == OpResult.Success)
			{
				return new HtmlString(
					$"<div id='errorProvider' class='message-success'{additionalAttrs}>{response.Message}</div>");
			}

			return new HtmlString(
				$"<div id='errorProvider' class='{HtmlHelper.ValidationSummaryCssClassName}'{additionalAttrs}>{response.Message}</div>");
		}

		public static string ToHTML(this List<string> list)
		{
			var text = new StringBuilder();
			text.Append("<ul>");
			foreach (var li in list)
				text.Append($"<li>{li}</li>");

			text.Append("</ul>");
			return text.ToString();
		}

		private static readonly Regex _valueAttributeCapture = new Regex(@"value=[""'](.*?)['""]", RegexOptions.IgnoreCase);
		public static string AntiForgeryTokenValue(this IHtmlHelper html)
		{
			var match = _valueAttributeCapture.Match(html.AntiForgeryToken().ToHtmlString());
			return match.Success ? match.Groups[1].Value : null;
		}

		public static string ToHtmlString(this IHtmlContent htmlContent)
		{
			if (htmlContent is HtmlString htmlString)
			{
				return htmlString.Value;
			}

			using (var writer = new StringWriter())
			{
				htmlContent.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
				return writer.ToString();
			}
		}

		public static string IsSelected(this IHtmlHelper html, string expectedAction, string expectedController, string selectClass = "active")
		{
			var currentAction = html.ViewContext.RouteData.Values["action"].ToString();
			var currentController = html.ViewContext.RouteData.Values["controller"].ToString();
			return string.Equals(currentAction, expectedAction, StringComparison.OrdinalIgnoreCase)
			       && string.Equals(currentController, expectedController, StringComparison.OrdinalIgnoreCase)
				? selectClass : string.Empty;
		}
	}
}

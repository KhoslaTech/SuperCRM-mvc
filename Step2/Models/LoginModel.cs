using System.ComponentModel.DataAnnotations;

namespace SuperCRM.Models
{
	public class LoginModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Display(Name = "Stay signed in?")]
		public bool RememberMe { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace SuperCRM.Models
{
	public class ChangeEmailModel
	{
		[Required]
		[EmailAddress]
		[MaxLength(100)]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}

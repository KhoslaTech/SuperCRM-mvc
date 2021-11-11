using System.ComponentModel.DataAnnotations;

namespace SuperCRM.Models
{
	public enum AccountType
	{
		Individual,
		Team,
	}

	[CustomValidation(typeof(RegisterModel), nameof(RegisterModel.IsValid))]
	public class RegisterModel
	{
		[Required]
		[MaxLength(60)]
		[Display(Name = "Name")]
		public string Name { get; set; }

		[Required]
		[MaxLength(100)]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[StringLength(512, MinimumLength = 6, ErrorMessage = "{0} must be between {1} and {2} characters.")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		public string ConfirmPassword { get; set; }

		[Display(Name = "Type of Account")]
		public AccountType Type { get; set; }

		[MaxLength(128)]
		[Display(Name = "Business Name")]
		public string BusinessName { get; set; }

		public static ValidationResult IsValid(RegisterModel model, ValidationContext context)
		{
			if (model.Type == AccountType.Team &&
			    string.IsNullOrWhiteSpace(model.BusinessName))
				return new ValidationResult("Business Name is required");

			return ValidationResult.Success;
		}
	}
}

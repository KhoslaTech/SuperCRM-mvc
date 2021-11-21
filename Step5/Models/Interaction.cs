using System;
using System.ComponentModel.DataAnnotations;
using SuperCRM.DataModels;

namespace SuperCRM.Models
{
	[CustomValidation(typeof(Interaction), nameof(Interaction.IsValid))]
	public class Interaction
	{
		public Guid? Id { get; set; }

		[Required]
		public Guid? ContactId { get; set; }

		[Required]
		public InteractionMethod? Method { get; set; }
		
		[MaxLength(256)]
		public string MethodDetails { get; set; }

		[Required]
		public string Notes { get; set; }

		public DateTime? InteractionDate { get; set; }

		public DateTime CreatedDate { get; set; }

		public string CreatedByName { get; set; }

		public static ValidationResult IsValid(Interaction model, ValidationContext context)
		{
			if (model.Method == InteractionMethod.Other &&
			    string.IsNullOrWhiteSpace(model.MethodDetails))
				return new ValidationResult("MethodDetails is required when Method specified as 'Other'.");

			return ValidationResult.Success;
		}
	}
}

using System;
using System.ComponentModel.DataAnnotations;

namespace SuperCRM.Models
{
	public class Contact
	{
		public Guid? Id { get; set; }

		[MaxLength(128)]
		[Required]
		public string Name { get; set; }

		[MaxLength(15)]
		public string Phone { get; set; }

		[MaxLength(75)]
		public string Email { get; set; }

		[MaxLength(128)]
		public string Address1 { get; set; }

		[MaxLength(128)]
		public string Address2 { get; set; }

		[MaxLength(64)]
		[Required]
		public string AcquiredFrom { get; set; }

		public string Notes { get; set; }

		public DateTime CreatedDate { get; set; }

		public string CreatedByName { get; set; }
	}
}
 
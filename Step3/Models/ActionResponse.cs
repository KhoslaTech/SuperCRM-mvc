using ASPSecurityKit;

namespace SuperCRM.Models
{
	public class ActionResponse
	{
		/// <summary>
		/// Indicates result of the operation by returning one of the results enumerated in <see cref="OpResult"/> 
		/// </summary>
		public OpResult Result { get; set; }

		/// <summary>
		/// Contains additional information (like error description) about the <see cref="Result"/> specifically if the operation did not complete successfully.
		/// </summary>
		public string Message { get; set; }
	}
}
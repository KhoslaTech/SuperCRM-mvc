// Copyright© 2020 Khosla Tech Private Limited. All rights reserved. 
// See License.txt in the project root for license information.

namespace SuperCRM.Security
{
	/// <summary>
	/// Enumerates commonly encountered sql errors comes up in TSql.
	/// </summary>
	public enum SqlErrors
	{
		/// <summary>
		/// Raised for both primary key and unique key
		/// </summary>
		KeyViolation = 2627,

		/// <summary>
		/// Raise when a unique index but not a key is violated.
		/// </summary>
		UniqueIndex = 2601,

		/// <summary>
		/// User defined errors thrown via raiserror
		/// </summary>
		UserDefined = 50000
	}
}

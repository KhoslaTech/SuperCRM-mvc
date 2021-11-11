using System.Collections.Generic;

namespace SuperCRM.Models
{
	public class ApiResponse
	{
		public bool Succeeded { get; protected set; }
		// for jtable json calls
		public string Result => this.Succeeded ? "OK" : "ERROR";

		public static ApiResponse Error(string errorMessage, params object[] args) => new ErrorApiResponse(string.Format(errorMessage, args));

		public static ApiResponse Success() => new SuccessApiResponse();

		public static ApiResponse Single(object record) => new SuccessSingleApiResponse(record);

		public static ApiResponse List<T>(PagedResult<T> result) where T : class => new SuccessListApiResponse(result.Records, result.TotalCount);

		public static ApiResponse List(IEnumerable<object> records, int totalRecordsCount) => new SuccessListApiResponse(records, totalRecordsCount);

		public static ApiResponse EmptyList() => new SuccessListApiResponse(new object[0], 0);

	}

	public class ErrorApiResponse : ApiResponse
	{
		public string Message { get; }

		public ErrorApiResponse(string message)
		{
			this.Message = message;
		}
	}

	public class SuccessApiResponse : ApiResponse
	{
		public SuccessApiResponse()
		{
			this.Succeeded = true;
		}
	}

	public class SuccessSingleApiResponse : SuccessApiResponse
	{
		public object Record { get; }

		public SuccessSingleApiResponse(object record)
		{
			this.Record = record;
		}
	}

	public class SuccessListApiResponse : SuccessApiResponse
	{
		public IEnumerable<object> Records { get; }
		public int TotalRecordCount { get; }

		public SuccessListApiResponse(IEnumerable<object> records, int totalRecordCount)
		{
			this.Records = records;
			this.TotalRecordCount = totalRecordCount;
		}
	}
}
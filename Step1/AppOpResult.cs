using ASPSecurityKit;

namespace SuperCRM
{
	public static class AppOpResult
	{
		// Add your custom error codes here. For example: 
		public const string UsernameAlreadyExists = nameof(UsernameAlreadyExists);

		// Add these error codes to the OpResult to HTTP status code mapper so the generic error handling logic can determine whether or not the error message is sensitive. E.G., anything with status other than 500 is considered non-sensitive and is expected to be reported.
		// By default code not added to the mapper is considered sensitive (500 status code) – so you can just skip such codes and mapp only the ones you want to reveal with original error message. Check out docs for OpException to learn more about this approach.
		static AppOpResult()
		{
			foreach (var o in new[] { UsernameAlreadyExists })
			{
				SecurityUtility.OpResultToHttpStatusCodeMapper.Add(o, 400);
			}
		}
	}
}

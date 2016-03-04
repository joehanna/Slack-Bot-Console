using System;

namespace Pook.SlackAPI.APIMessages
{
	public abstract class Response
	{
		/// <summary>
		/// Should always be checked before trying to process a response.
		/// </summary>
		public bool ok;

		/// <summary>
		/// if ok is false, then this is the reason-code
		/// </summary>
		public string error;

		public void AssertOk()
		{
			if (!ok)
				throw new InvalidOperationException($"An error occurred: {error}");
		}
	}
}
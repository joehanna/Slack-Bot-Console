using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Pook.SlackAPI
{
	public class RetryPolicy
	{
		public RetryPolicy(IEnumerable<int> retryTimings = null, Func<Exception, HttpResponseMessage, bool> isTransientError = null)
		{
			if (retryTimings != null)
				this.retryTimings = retryTimings;

			this.isTransientError = isTransientError;
		}

		public RetryPolicy(Func<HttpResponseMessage, int> getDelayFromResponse, Func<Exception, HttpResponseMessage, bool> isTransientError = null) : this(isTransientError: isTransientError)
		{
			this.getDelayFromResponse = getDelayFromResponse;
		}

		/// <summary>
		/// Used if an error has occured and retries are being attempted to determine if the error is a transient one from the exception/response message
		/// </summary>
		private readonly Func<Exception, HttpResponseMessage, bool> isTransientError = null;
		/// <summary>
		/// Used if the response returns a non success code and some information governing how long until the application should again attempt the request
		/// </summary>
		private readonly Func<HttpResponseMessage, int> getDelayFromResponse = null;
		/// <summary>
		/// Timings used for the delays. When the last item is reached the retry attempts will stop.
		/// Not used if getDelayFromResponse is not null
		/// </summary>
		private readonly IEnumerable<int> retryTimings = new[] { 0, 1, 2 };

		/// <summary>
		/// Takes the exception and response of a failed http request to determine if a retry should be attempted. If successful gives the delay in seconds using the out parameter delay
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="response"></param>
		/// <param name="attemptNumber"></param>
		/// <param name="delay"></param>
		/// <returns></returns>
		public bool AttemptRetry(Exception ex, HttpResponseMessage response, int attemptNumber, out int delay)
		{
			delay = -1;

			// If an isTransientError handler has been defined then use it, otherwise assume the error is transient
			var isTransientErrorResult = isTransientError?.Invoke(ex, response) ?? true;
			if (!isTransientErrorResult)
				return false;

			if (getDelayFromResponse != null)
			{
				delay = getDelayFromResponse.Invoke(response);
				return true;
			}

			// Check retryTimings enumerable and get the delay
			if (attemptNumber > retryTimings.Count())
				return false;

			delay = retryTimings.Skip(attemptNumber - 1).Take(1).First();
			return true;
		}
	}
}
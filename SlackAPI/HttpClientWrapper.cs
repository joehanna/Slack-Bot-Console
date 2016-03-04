using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace Pook.SlackAPI
{
	/// <summary>
	/// Provides a convenient api to HttpClient.
	/// Hides Synchronization context issues with "ConfigureAwait(false)"
	/// Allows for retry.
	/// </summary>
	public partial class HttpClientWrapper : IDisposable
	{
		public HttpClientWrapper() : this(new HttpClient()) { }
		/// <summary>
		/// Uses the provided HttpClient in this Wrapper
		/// </summary>
		/// <param name="http"></param>
		public HttpClientWrapper(HttpClient http) { this.http = http; }

		private readonly HttpClient http;

		public static MediaTypeFormatter[] DefaultFormatters =
		{
			new JsonMediaTypeFormatter(),
			new XmlMediaTypeFormatter(),
			new FormUrlEncodedMediaTypeFormatter(),
			new JQueryMvcFormUrlEncodedFormatter()
		};

		public MediaTypeFormatter[] MediaTypeFormatters = DefaultFormatters;
		public MediaTypeFormatter DefaultMediaTypeFormatter => MediaTypeFormatters[0];
		public RetryPolicy RetryPolicy { get; set; } = null;


		/// <summary>
		/// Sets the time to wait before the request timesout
		/// <para>
		/// CAUTION: This will change the timeout for all subsequent requests, including those from other threads
		/// </para>
		/// </summary>
		public TimeSpan Timeout { get { return http.Timeout; } set { http.Timeout = value; } }
		public Uri BaseAddress => http.BaseAddress;

		public HttpContent CreateContent<T>(T item)
		{
			return new ObjectContent<T>(item, DefaultMediaTypeFormatter);
		}
		public HttpContent CreateContent<T>(T item, MediaTypeFormatter mediaTypeFormatter)
		{
			return new ObjectContent<T>(item, mediaTypeFormatter);
		}

		public Task<T> ReadContent<T>(HttpResponseMessage response)
		{
			return ReadContent<T>(response.Content);
		}
		public Task<T> ReadContent<T>(HttpContent content)
		{
			return content.ReadAsAsync<T>(MediaTypeFormatters);
		}

		public Task<T> GetAsync<T>(string uri = null)
		{
			return SendAsync<T>(uri, HttpMethod.Get);
		}
		public Task<HttpResponseMessage> GetAsync(string uri = null)
		{
			return SendAsync(uri, HttpMethod.Get);
		}

		public Task<TResponse> PostAsync<TResponse, TRequest>(string uri = null, TRequest item = default(TRequest))
		{
			return SendAsync<TResponse>(uri, HttpMethod.Post, () => CreateContent(item));
		}
		public Task<TResponse> PostAsync<TResponse>(string uri = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync<TResponse>(uri, HttpMethod.Post, contentFactory);
		}
		public Task<HttpResponseMessage> PostAsync(string uri = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync(uri, HttpMethod.Post, contentFactory);
		}
		public Task<HttpResponseMessage> PostAsync<T>(string url, T item)
		{
			return SendAsync(url, HttpMethod.Post, () => CreateContent(item));
		}

		public Task<TResponse> PutAsync<TResponse, TRequest>(string uri = null, TRequest item = null) where TRequest : class
		{
			return SendAsync<TResponse>(uri, HttpMethod.Put, () => CreateContent(item));
		}
		public Task<TResponse> PutAsync<TResponse>(string uri = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync<TResponse>(uri, HttpMethod.Put, contentFactory);
		}
		public Task<HttpResponseMessage> PutAsync(string uri = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync(uri, HttpMethod.Put, contentFactory);
		}
		public Task<TResponse> DeleteAsync<TResponse, TRequest>(string uri = null, TRequest item = null) where TRequest : class
		{
			return SendAsync<TResponse>(uri, HttpMethod.Delete, () => CreateContent(item));
		}
		public Task<TResponse> DeleteAsync<TResponse>(string uri = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync<TResponse>(uri, HttpMethod.Delete, contentFactory);
		}
		public Task<HttpResponseMessage> DeleteAsync(string uri = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync(uri, HttpMethod.Delete, contentFactory);
		}

		public async Task<TResponse> SendAsync<TResponse>(string uri = null, HttpMethod httpMethod = null, Func<HttpContent> contentFactory = null)
		{
			var response = await SendAsync(() => CreateRequest(uri, httpMethod, contentFactory)).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return await ReadContent<TResponse>(response.Content).ConfigureAwait(false);
		}
		public Task<HttpResponseMessage> SendAsync(string uri = null, HttpMethod httpMethod = null, Func<HttpContent> contentFactory = null)
		{
			return SendAsync(() => CreateRequest(uri, httpMethod, contentFactory));
		}

		/// <summary>
		/// The requestFactory MUST recreate the content if retrying is being used.
		/// Each time a request is made it's content is disposed of making resending the same request impossible.
		/// </summary>
		/// <param name="requestFactory"></param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> requestFactory)
		{
			var attemptNumber = 0;
			HttpResponseMessage response = null;

			while (true)
			{
				try
				{
					attemptNumber++;

					// Use the requestFactory to recreate the request and content
					var request = requestFactory();
					response = await SendAsync(request).ConfigureAwait(false);
					response.EnsureSuccessStatusCode();
					return response;
				}
				catch (Exception ex)
				{
					int delay;
					if (RetryPolicy == null)
						throw;
					if (!RetryPolicy.AttemptRetry(ex, response, attemptNumber, out delay))
						throw;

					await Task.Delay(delay * 1000);
				}
			}
		}

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			return http.SendAsync(request);
		}

		public HttpRequestMessage CreateRequest(string uri = null, HttpMethod httpMethod = null, Func<HttpContent> contentFactory = null)
		{
			return CreateRequest(uri, httpMethod, contentFactory?.Invoke());
		}
		public HttpRequestMessage CreateRequest(string uri = null, HttpMethod httpMethod = null, HttpContent content = null)
		{
			if (string.IsNullOrWhiteSpace(uri))
				uri = string.Empty;

			var request = new HttpRequestMessage(httpMethod ?? HttpMethod.Get, uri);

			if (content != null)
				request.Content = content;

			return request;
		}

		public async Task<Stream> GetStreamAsync(string url = null)
		{
			return await http.GetStreamAsync(url ?? string.Empty).ConfigureAwait(false);
		}

		public void Dispose()
		{
			http.Dispose();
		}

		#region Multipart Content
		public static HttpContent CreateContent(Action<MultipartContentBuilder> buildFunc)
		{
			var builder = new MultipartContentBuilder();
			buildFunc(builder);
			return builder.Content;
		}
		public static MultipartContentBuilder CreateMultipartContent()
		{
			return new MultipartContentBuilder();
		}

		public class MultipartContentBuilder
		{
			private readonly MultipartContent content = new MultipartContent("mixed");
			public MultipartContent Content { get { return content; } }

			public MultipartContentBuilder AddContent(HttpContent item)
			{
				content.Add(item);
				return this;
			}
			public MultipartContentBuilder AddStream(Stream item)
			{
				content.Add(new StreamContent(item));
				return this;
			}

			public MultipartContentBuilder Add<T>(T item)
			{
				content.Add(new ObjectContent(typeof(T), item, new JsonMediaTypeFormatter()));
				return this;
			}

			public HttpRequestMessage GetRequest(string url)
			{
				return new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
			}

			public static implicit operator HttpContent(MultipartContentBuilder builder)
			{
				return builder.Content;
			}
		}
		#endregion
	}
}
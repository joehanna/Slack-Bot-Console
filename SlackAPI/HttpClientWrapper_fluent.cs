using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

// ReSharper disable ParameterHidesMember

namespace Pook.SlackAPI
{
	public partial class HttpClientWrapper
	{
		public static Fluent Create()
		{
			return new Fluent();
		}

		public static Fluent Create(Uri baseAddress, bool ensureTrailingSlash = false)
		{
			return new Fluent().WithBaseAdderss(baseAddress, ensureTrailingSlash);
		}

		public static Fluent Create(string baseAddress, bool ensureTrailingSlash = false)
		{
			return new Fluent().WithBaseAdderss(baseAddress, ensureTrailingSlash);
		}

		public sealed class Fluent
		{
			private Action<HttpClient> httpConfig = delegate { };
			private Action<HttpClientHandler> httpHandlerConfig = delegate { };

			private Uri baseAddress;
			private HttpMessageHandler handler;
			private IEnumerable<DelegatingHandler> handlers;
			private MediaTypeFormatter[] formatters;

			private HttpClient CreateHttp()
			{
				if (handler == null)
					handler = new HttpClientHandler();
				var clientHandler = handler as HttpClientHandler;
				if (clientHandler != null)
					httpHandlerConfig?.Invoke(clientHandler);

				var pipeline = HttpClientFactory.CreatePipeline(handler, handlers);
				var http = new HttpClient(pipeline) { BaseAddress = baseAddress };
				httpConfig?.Invoke(http);

				return http;
			}
			private HttpClientWrapper CreateHttpWrapper()
			{
				var http = CreateHttp();
				var wrapper = new HttpClientWrapper(http);
				if (formatters != null)
					wrapper.MediaTypeFormatters = formatters;

				return wrapper;
			}

			public Fluent WithBaseAdderss(Uri baseAddress, bool ensureTrailingSlash = false)
			{
				if (baseAddress != null && ensureTrailingSlash && !baseAddress.AbsolutePath.EndsWith("/"))
				{
					var builder = new UriBuilder(baseAddress);
					builder.Path += "/";
					baseAddress = builder.Uri;
				}
				this.baseAddress = baseAddress;
				return this;
			}
			public Fluent WithBaseAdderss(string baseAddress, bool ensureTrailingSlash = false)
			{
				return WithBaseAdderss(new Uri(baseAddress), ensureTrailingSlash);
			}

			public Fluent WithHandler(HttpMessageHandler handler)
			{
				this.handler = handler;

				return this;
			}
			public Fluent WithHandlers(HttpMessageHandler handler, params DelegatingHandler[] handlers)
			{
				this.handler = handler;
				this.handlers = handlers;

				return this;
			}
			public Fluent WithHandlers(HttpMessageHandler handler, IEnumerable<DelegatingHandler> handlers)
			{
				this.handler = handler;
				this.handlers = handlers;

				return this;
			}
			public Fluent WithHandlers(params DelegatingHandler[] handlers)
			{
				this.handlers = handlers;

				return this;
			}
			public Fluent WithHandlers(IEnumerable<DelegatingHandler> handlers)
			{
				this.handlers = handlers;

				return this;
			}

			public Fluent WithFormatters(params MediaTypeFormatter[] formatters)
			{
				this.formatters = formatters;
				return this;
			}
			public Fluent WithFormatters(IEnumerable<MediaTypeFormatter> formatters)
			{
				this.formatters = formatters.ToArray();
				return this;
			}

			/// <summary>
			/// Configure client to send "application/json" in the Accept header
			/// </summary>
			/// <returns></returns>
			public Fluent AcceptJson()
			{
				httpConfig += h =>
				{
					h.DefaultRequestHeaders.Accept.Clear();
					h.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				};

				return this;
			}

			public Fluent SetTimeout(int ms)
			{
				httpConfig += h => { h.Timeout = TimeSpan.FromMilliseconds(ms); };
				return this;
			}
			public Fluent SetTimeout(TimeSpan timespan)
			{
				httpConfig += h => { h.Timeout = timespan; };
				return this;
			}

			public Fluent ConfigureClient(Action<HttpClient> config)
			{
				if (config != null)
					httpConfig += config;

				return this;
			}
			public Fluent ConfigureClientHandler(Action<HttpClientHandler> config)
			{
				if (config != null)
					httpHandlerConfig += config;

				return this;
			}

			public static implicit operator HttpClientWrapper(Fluent fluent)
			{
				return fluent.CreateHttpWrapper();
			}
			public static implicit operator HttpClient(Fluent fluent)
			{
				return fluent.CreateHttp();
			}
		}
	}
}
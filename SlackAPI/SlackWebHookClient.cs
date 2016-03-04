using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pook.SlackAPI
{
	public class SlackWebHookClient : IDisposable
	{
		public SlackWebHookClient(string uri)
		{
			client =
				HttpClientWrapper
				.Create(uri)
				.ConfigureClient(http =>
				{
					http.DefaultRequestHeaders.Accept.Clear();
					http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				});
		}

		private readonly HttpClientWrapper client;

		public event Action<IncomingWebHookMessage> MessageSent = delegate { };

		public string Channel { get; set; }

		public string Username { get; set; }

		public async Task SendMessage(string text, string channel = null, string username = null)
		{
			var message = new IncomingWebHookMessage
			{
				text = text,
				channel = channel,
				username = username ?? Username
			};

			await client.PostAsync(string.Empty, message);

			if (MessageSent != null)
			{
				var messageSent = MessageSent;
				messageSent(message);
			}
		}

		public void Dispose()
		{
			client?.Dispose();
		}
	}
}
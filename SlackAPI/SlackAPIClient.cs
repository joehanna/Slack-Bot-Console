using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Pook.SlackAPI.APIMessages;

namespace Pook.SlackAPI
{
	public class SlackAPI : ISlackAPI
	{
		private const string DefaultAddress = "https://slack.com/api/";

		public SlackAPI(string token, string baseAddress = null)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));
			Token = token;
			
			client = 
				HttpClientWrapper
				.Create(baseAddress ?? DefaultAddress)
				.ConfigureClient(http =>
				{
					http.DefaultRequestHeaders.Accept.Clear();
					http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				});
		}

		private readonly HttpClientWrapper client;
		public string Token { get; }

		public Task<LoginResponse> GetRtmLogin()
		{
			return client.GetAsync<LoginResponse>("rtm.start?token=" + Token);
		}

		public Task<ChatUpdateResponse> ChatUpdate(string ts, string channel, string newText)
		{
			return client.PostAsync<ChatUpdateResponse, ChatUpdate>("chat.update", new ChatUpdate
			{
				token = Token,
				ts = ts,
				channel = channel,
				text = newText,
				link_names = true
			});
		}

		public Task<ChatMessageResponse> ChatPostMessage(ChatMessage message)
		{
			message.token = Token;
			return client.PostAsync<ChatMessageResponse, ChatMessage>("chat.postMessage", message);
		}
	}
}
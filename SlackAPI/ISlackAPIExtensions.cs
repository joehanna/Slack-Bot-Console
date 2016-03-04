using System.Threading.Tasks;

using Pook.SlackAPI.APIMessages;

namespace Pook.SlackAPI
{
	public static class ISlackAPIExtensions
	{
		public static Task<ChatUpdateResponse> ChatUpdate(this ISlackAPI client, Message originalMessage, string newText)
		{
			return client.ChatUpdate(originalMessage.ts, originalMessage.channel, newText);
		}
		public static Task<ChatMessageResponse> ChatPostMessage(this ISlackAPI client, string channel, string text)
		{
			return client.ChatPostMessage(new ChatMessage
			{
				channel = channel,
				text = text
			});
		}
	}
}
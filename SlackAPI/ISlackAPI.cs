using System.Threading.Tasks;

using Pook.SlackAPI.APIMessages;

namespace Pook.SlackAPI
{
	public interface ISlackAPI
	{
		string Token { get; }

		Task<LoginResponse> GetRtmLogin();
		Task<ChatMessageResponse> ChatPostMessage(ChatMessage message);
		Task<ChatUpdateResponse> ChatUpdate(string ts, string channel, string newText);
	}
}
// ReSharper disable InconsistentNaming
namespace Pook.SlackAPI.APIMessages
{
	public class ChatMessage
	{
		public string token;
		public string channel;
		public string text;
		public string username;
		public bool as_user;
		public string parse;
		public string link_names;
		public string attatchments;
		public bool unfurl_Links;
		public bool unfurl_media;
		public string icon_url;
		public string icon_emoji;
	}
}
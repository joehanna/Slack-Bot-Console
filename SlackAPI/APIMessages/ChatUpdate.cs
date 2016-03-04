namespace Pook.SlackAPI.APIMessages
{
	public class ChatUpdate
	{
		public string token;
		public string ts;
		public string channel;
		public string text;
		public string attachments;
		public string parse;
		public bool link_names;
	}
}
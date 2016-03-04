using System;

namespace Pook.SlackAPI.RTMMessages
{
	public class Message : SlackSocketMessage
	{
		public string user;
		public string channel;
		public string text;
		public string team;
		public string ts;

		public Message()
		{
			type = "message";
		}
	}
}
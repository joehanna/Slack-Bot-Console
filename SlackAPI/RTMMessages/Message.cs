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

    public class BotMessage : Message
    {
        public string subtype => "bot_message";
        public string bot_id;
    }
}
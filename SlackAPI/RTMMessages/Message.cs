using System;
using System.Collections.Generic;

namespace Pook.SlackAPI.RTMMessages
{
	public class Message : SlackSocketMessage
	{
        public virtual string subtype { get; set; }
        public string user;
		public string channel;
		public string team;
		public string ts;
		public string text;
        public bool mrkdwn;

        public IList<MessageAttachment> attachments;

		public Message()
		{
			type = "message";
		}
	}

    public class MessageAttachment
    {
        public string title;
        public string text;
        public bool mrkdwn;
        public string fallback;
    }

    public class BotMessage : Message
    {
        public override string subtype => "bot_message";
        public string bot_id;
    }
}
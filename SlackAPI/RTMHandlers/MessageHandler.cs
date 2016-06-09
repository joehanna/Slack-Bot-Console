using System;
using System.Diagnostics;

using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI.RTMHandlers
{
	[SlackSocketRouting("message")]
	public class MessageHandler : IEventHandler<Message>
	{
		public void Handle(ISlackSocket socket, Message message)
		{
            if (string.IsNullOrEmpty(message?.text))
            {
                Debug.WriteLine("Empty message");
                return;
            }

            Debug.WriteLine("Message: " + message.text);

            var user = socket.State.GetUser(message.user);
			foreach (var responder in socket.Responders)
			{
				if (responder.CanRespond(message, user))
					responder.Respond(socket, message, user);
			}
		}
	}
}
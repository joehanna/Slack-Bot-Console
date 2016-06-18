using System;
using System.Threading.Tasks;
using System.ComponentModel;

using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI.RTMResponders
{
	[Description("!")]
	public class EchoResponder : IMessageResponder
	{
		public bool CanRespond(Message message, SlackUser user)
		{
			return message.text.StartsWith("echo", StringComparison.OrdinalIgnoreCase);
		}

		public Task Respond(ISlackSocket socket, Message message, SlackUser user)
		{
			return socket.Send(message.Reply("ECHO: " + message.text.Replace("echo", string.Empty)));
		}
	}
}
using System;
using System.Threading.Tasks;

using Pook.SlackAPI;
using Pook.SlackAPI.RTMMessages;

namespace SlackConsole
{
	public class HelloResponder : IMessageResponder
	{
		public bool CanRespond(Message message, SlackUser user)
		{
			return message.text.StartsWith("hello", StringComparison.InvariantCultureIgnoreCase);
		}

		public Task Respond(ISlackSocket socket, Message message, SlackUser user)
		{
			socket.Send(message.Reply("hey"));
			return Task.FromResult(0);
		}
	}
}
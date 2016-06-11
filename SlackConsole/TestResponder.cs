using System;
using System.Threading.Tasks;

using Pook.SlackAPI;
using Pook.SlackAPI.RTMMessages;

namespace SlackConsole
{
	public class TestResponder : IMessageResponder
	{
		public bool CanRespond(Message message, SlackUser user)
		{
			return message.text.StartsWith("test", StringComparison.InvariantCultureIgnoreCase);
		}

		public async Task Respond(ISlackSocket socket, Message message, SlackUser user)
		{
			await socket.Send(message.Reply("what about step 2? Enter '3' to continue"));
			user.NextStep = Step2;
		}

		public async Task<bool> Step2(ISlackSocket socket, Message message, SlackUser user)
		{
			if (message.text != "2")
			{
				await socket.Send(message.Reply("oh, I didn't expect that. Lets start again"));
				return false;
			}

			await socket.Send(message.Reply("what about step 3? Enter '3' to continue"));
			user.NextStep = Step3;
			return true;
		}

		public async Task<bool> Step3(ISlackSocket socket, Message message, SlackUser user)
		{
			if (message.text != "3")
			{
				await socket.Send(message.Reply("oh, I didn't expect that. Lets start again"));
				return false;
			}

			await socket.Send(message.Reply("ok that's the end. You're back at the beginning..."));
			user.NextStep = null;
			return true;
		}
	}
}
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

    public class TestResponder : IMessageResponder
    {
        public bool CanRespond(Message message, SlackUser user)
        {
            return message.text.StartsWith("test", StringComparison.InvariantCultureIgnoreCase);
        }

        public Task Respond(ISlackSocket socket, Message message, SlackUser user)
        {
            socket.Send(message.Reply("what about step 2? Enter '3' to continue"));
            user.NextStep = Step2;
            return Task.FromResult(0);
        }

        public Task<bool> Step2(ISlackSocket socket, Message message, SlackUser user)
        {
            if (message.text != "2")
            {
                socket.Send(message.Reply("oh, I didn't expect that. Lets start again"));
                return Task.FromResult(false);
            }

            socket.Send(message.Reply("what about step 3? Enter '3' to continue"));
            user.NextStep = Step3;
            return Task.FromResult(true);
        }

        public Task<bool> Step3(ISlackSocket socket, Message message, SlackUser user)
        {
            if (message.text != "3")
            {
                socket.Send(message.Reply("oh, I didn't expect that. Lets start again"));
                return Task.FromResult(false);
            }

            socket.Send(message.Reply("ok that's the end. You're back at the beginning..."));
            user.NextStep = null;
            return Task.FromResult(true);
        }
    }
}
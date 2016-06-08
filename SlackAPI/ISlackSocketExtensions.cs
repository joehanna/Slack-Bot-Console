using Pook.SlackAPI.RTMMessages;
using System.Linq;

namespace Pook.SlackAPI
{
    public static class ISlackSocketExtensions
    {
        public static void SendSelf(this ISlackSocket socket, string message)
        {
            var msg = new Message
            {
                user = socket.State.Self.id,
                channel = socket.State.Self.id,
                text = message
            };
            socket.Send(msg);
        }
        public static Message Reply(this Message msg, string message)
        {
            return new Message
            {
                reply_to = msg.id,
                channel = msg.channel,
                text = message
            };
        }
    }
}
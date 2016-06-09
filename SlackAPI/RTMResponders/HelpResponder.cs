using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Pook.SlackAPI;
using Pook.SlackAPI.RTMMessages;

namespace SlackAPI.RTMResponders
{
    [Description("Displays some help")]
    public class HelpResponder : IMessageResponder
    {
        public bool CanRespond(Message message, SlackUser user)
        {
            return message.text.StartsWith("help", StringComparison.OrdinalIgnoreCase);
        }

        public Task Respond(ISlackSocket socket, Message message, SlackUser user)
        {
            var response = new StringBuilder();
            foreach (var r in socket.Responders)
            {
                var attr = (DescriptionAttribute)r.GetType().GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                string name = r.GetType().Name.Replace("Responder", string.Empty);
                response.AppendLine($"*{name}* {attr?.Description}");
            }

            socket.Send(message.Reply("Here's is what I know \n>>>" + response.ToString()));

            return Task.FromResult(0);
        }
    }
}

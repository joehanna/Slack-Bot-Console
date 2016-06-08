using System.Diagnostics;
using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI.RTMHandlers
{
    [SlackSocketRouting("presence_change")]
    public class PresenceChangeHandler : IEventHandler<PresenceChange>
    {
        public void Handle(ISlackSocket socket, PresenceChange message)
        {
            var user = socket.State.GetUser(message.user);
            if (user == null)
                return;
            Debug.WriteLine($"{user.Name}<{user.Id}> presence: {user.Presence}->{message.presence}");
            user.Presence = message.presence;
        }
    }
}
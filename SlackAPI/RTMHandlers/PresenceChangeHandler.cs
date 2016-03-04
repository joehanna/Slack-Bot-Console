using System;
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

			Debug.WriteLine($"{user.name}<{user.id}> presence: {user.presence}->{message.presence}");
			user.presence = message.presence;
		}
	}
}
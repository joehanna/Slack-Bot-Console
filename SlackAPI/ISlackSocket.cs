using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI
{
	public interface ISlackSocket
	{
		SlackState State { get; }
		ISlackAPI API { get; }
		IReadOnlyCollection<IMessageResponder> Responders { get; }

		Task Send(SlackSocketMessage message);
		Task Send(Message message, Action<ISlackSocket, Message, SlackUser> callback);
	}
}
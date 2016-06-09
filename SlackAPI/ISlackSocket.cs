using Pook.SlackAPI.RTMMessages;
using System;
using System.Collections.Generic;

namespace Pook.SlackAPI
{
	public interface ISlackSocket
	{
		SlackState State { get; }
		ISlackAPI API { get; }
        IReadOnlyCollection<IMessageResponder> Responders { get; }

		void Send(SlackSocketMessage message);
        void Send(Message message, Action<ISlackSocket, Message, SlackUser> callback);
    }
}
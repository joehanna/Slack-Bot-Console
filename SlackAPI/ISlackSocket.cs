using System;
using System.Collections.Generic;

namespace Pook.SlackAPI
{
	public interface ISlackSocket
	{
		SlackState State { get; }
		ISlackAPI API { get; }
        IReadOnlyCollection<IMessageResponder> Responders { get; }

        void Send<T>(SlackSocketMessage message, Action<T> callback) where T : SlackSocketMessage;
		void Send(SlackSocketMessage message);
	}
}
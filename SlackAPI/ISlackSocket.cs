using System;

namespace Pook.SlackAPI
{
	public interface ISlackSocket
	{
		StackState State { get; }
		ISlackAPI API { get; }
		void Send<T>(SlackSocketMessage message, Action<T> callback) where T : SlackSocketMessage;
		void Send(SlackSocketMessage message);
	}
}
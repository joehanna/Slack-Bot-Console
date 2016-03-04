using System;

using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI.RTMHandlers
{
	[SlackSocketRouting("hello")]
	public class HelloHandler : IEventHandler<Hello>
	{
		public void Handle(ISlackSocket socket, Hello message)
		{
			Console.WriteLine("Hello: " + message.ok);
			socket.State.HelloReceived = true;
		}
	}
}
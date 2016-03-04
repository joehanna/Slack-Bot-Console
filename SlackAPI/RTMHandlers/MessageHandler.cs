using System;
using System.Collections.Generic;
using System.Linq;

using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI.RTMHandlers
{
	[SlackSocketRouting("message")]
	public class MessageHandler : IEventHandler<Message>
	{
		static readonly List<IMessageResponder> responders = new List<IMessageResponder>();

		static MessageHandler()
		{
			foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assy.GlobalAssemblyCache)
					continue;

				foreach (var t in assy.GetTypes())
				{
					if (t.GetInterfaces().Any(i => i == typeof(IMessageResponder)))
						responders.Add((IMessageResponder)Activator.CreateInstance(t));
				}
			}
		}

		public void Handle(ISlackSocket socket, Message message)
		{
			Console.WriteLine("Message: " + message.text);

			foreach (var responder in responders)
			{
				if (responder.CanRespond(message))
					responder.Respond(socket, message);
			}
		}
	}
}
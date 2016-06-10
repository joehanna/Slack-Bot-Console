using System;
using System.Linq;
using System.Reflection;

namespace Pook.SlackAPI
{
	public static class SlackSocketResponderExtensions
	{
		public static SlackSocket AddResponder<T>(this SlackSocket socket)
		{
			return socket.AddResponder(typeof(T));
		}

		public static SlackSocket AddResponder(this SlackSocket socket, Type responderType)
		{
			if (responderType.GetInterfaces().Any(i => i == typeof(IMessageResponder)))
				socket.AddResponder((IMessageResponder)Activator.CreateInstance(responderType));
			return socket;
		}

		public static SlackSocket AddRespondersFromAssembly(this SlackSocket socket, Assembly assy)
		{
			if (assy.GlobalAssemblyCache)
				return socket;
			foreach (var t in assy.GetTypes())
			{
				if (t.GetInterfaces().Any(i => i == typeof(IMessageResponder)))
					socket.AddResponder(t);
			}

			return socket;
		}

		public static SlackSocket AddBuiltinResponders(this SlackSocket socket)
		{
			return socket.AddRespondersFromAssembly(typeof(SlackSocket).Assembly);
		}

		public static SlackSocket AddAllResponders(this SlackSocket socket)
		{
			foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assy.GlobalAssemblyCache)
					continue;
				foreach (var t in assy.GetTypes())
				{
					if (t.GetInterfaces().Any(i => i == typeof(IMessageResponder)))
						socket.AddResponder((IMessageResponder)Activator.CreateInstance(t));
				}
			}

			return socket;
		}
	}
}
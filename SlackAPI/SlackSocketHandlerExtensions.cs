using System;
using System.Reflection;

namespace Pook.SlackAPI
{
	public static class SlackSocketHandlerExtensions
	{
		public static SlackSocket AddEventHandler<T>(this SlackSocket socket)
		{
			return socket.AddEventHandler(typeof(T));
		}

		public static SlackSocket AddEventHandlerFromAssembly(this SlackSocket socket, Assembly assy)
		{
			if (assy.GlobalAssemblyCache)
				return socket;
			foreach (var t in assy.GetTypes())
				socket.AddEventHandler(t);
			return socket;
		}

		public static SlackSocket AddEventHandlersFromAssembly<T>(this SlackSocket socket)
		{
			return socket.AddEventHandlerFromAssembly(typeof(T).Assembly);
		}

		public static SlackSocket AddBuiltinEventHandlers(this SlackSocket socket)
		{
			return socket.AddEventHandlerFromAssembly(typeof(SlackSocket).Assembly);
		}

		public static SlackSocket AddAllEventHandlers(this SlackSocket socket)
		{
			foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
				socket.AddEventHandlerFromAssembly(assy);
			return socket;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Newtonsoft.Json;

namespace Pook.SlackAPI
{
	public class SlackSocket : ISlackSocket
	{
		static readonly Dictionary<string, SlackEventHandler> handlers = new Dictionary<string, SlackEventHandler>();

		static SlackSocket()
		{
			foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assy.GlobalAssemblyCache)
					continue;

				foreach (var t in assy.GetTypes())
				{
					bool isHandler = t.GetInterfaces()
						.Where(i => i.IsGenericType)
						.Any(i => i.GetGenericTypeDefinition() == typeof(IEventHandler<>));
					if (!isHandler)
						continue;

					foreach (var route in t.GetCustomAttributes<SlackSocketRoutingAttribute>())
						handlers.Add(route.Type, new SlackEventHandler(t));
				}
			}
		}

		public SlackSocket(string token, string baseAddress = null)
		{
			socket = new ClientWebSocket();
			currentId = 1;
			cts = new CancellationTokenSource();
			API = new SlackAPI(token, baseAddress);
		}

		readonly CancellationTokenSource cts;
		private readonly Dictionary<int, Action<string>> callbacks = new Dictionary<int, Action<string>>();

		private QueueHandler<string> sendQueue;
		private QueueHandler<string> handlerQueue;

		int closedEmitted;

		private readonly ClientWebSocket socket;
		int currentId;

		public StackState State { get; private set; }
		public ISlackAPI API { get; }

		public event Action Connected;
		public event Action<WebSocketException> ErrorSending;
		public event Action<WebSocketException> ErrorReceiving;
		public event Action ConnectionClosed;

		public bool IsConnected => socket != null && socket.State == WebSocketState.Open;

		public async Task Login()
		{
			Debug.WriteLine("rtm.start");
			var login = await API.GetRtmLogin();
			Debug.WriteLine("RTM Login: " + login.url);
			State = new StackState(login);
			
			handlerQueue = QueueHandler<string>.Start(HandleItem, cts.Token, new ParallelOptions { MaxDegreeOfParallelism = 5 });
			sendQueue = QueueHandler<string>.Start(SendItem, cts.Token, new ParallelOptions { MaxDegreeOfParallelism = 5 });

			socket.ConnectAsync(new Uri(login.url), cts.Token).Wait();
			Debug.WriteLine("RTM: connected");
			Connected?.Invoke();

			StartSocketLoop();
		}

		public void Send<T>(SlackSocketMessage message, Action<T> callback)
				where T : SlackSocketMessage
		{
			message.id = Interlocked.Increment(ref currentId);
			callbacks.Add(message.id, c =>
			{
				T obj = JsonConvert.DeserializeObject<T>(c, new UnixDateTimeConverter());
				callback(obj);
			});
			Send(message);
		}
		public void Send(SlackSocketMessage message)
		{
			if (message.id == 0)
				message.id = Interlocked.Increment(ref currentId);

			if (string.IsNullOrEmpty(message.type))
			{
				var route = message.GetType().GetCustomAttributes<SlackSocketRoutingAttribute>().LastOrDefault();
				if (route == null)
					throw new InvalidProgramException("Cannot send without a proper route!");

				message.type = route.Type;
			}

			sendQueue.Add(JsonConvert.SerializeObject(message, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
		}

		//public void BindCallback<T>(Action<T> callback)
		//{
		//	Type t = typeof(T);

		//	foreach (SlackSocketRouting route in t.GetCustomAttributes<SlackSocketRouting>())
		//	{
		//		if (!routes.ContainsKey(route.Type))
		//			routes.Add(route.Type, new Dictionary<string, Delegate>());
		//		if (!routes[route.Type].ContainsKey(route.SubType ?? "null"))
		//			routes[route.Type].Add(route.SubType ?? "null", callback);
		//		else
		//			routes[route.Type][route.SubType ?? "null"] = Delegate.Combine(routes[route.Type][route.SubType ?? "null"], callback);
		//	}
		//}

		//public void UnbindCallback<T>(Action<T> callback)
		//{
		//	Type t = typeof(T);
		//	foreach (SlackSocketRouting route in t.GetCustomAttributes<SlackSocketRouting>())
		//	{
		//		Delegate d = routes.ContainsKey(route.Type) ? (routes.ContainsKey(route.SubType ?? "null") ? routes[route.Type][route.SubType ?? "null"] : null) : null;
		//		if (d != null)
		//		{
		//			Delegate newd = Delegate.Remove(d, callback);
		//			routes[route.Type][route.SubType ?? "null"] = newd;
		//		}
		//	}
		//}

		private void StartSocketLoop()
		{
			Task.Factory
				.StartNew(async () => await SocketLoop(), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
				.Unwrap()
				.ContinueWith(t =>
				{
					if (t.Exception != null)
						Trace.TraceError(t.Exception.Message);
					Trace.TraceInformation("SocketLoop finished");
				});
		}
		private async Task SocketLoop()
		{
			var bytes = new byte[1024];
			var msg = new StringBuilder();
			while (socket.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result;
				try
				{
					result = await socket.ReceiveAsync(new ArraySegment<byte>(bytes), cts.Token);
					msg.Append(Encoding.UTF8.GetString(bytes, 0, result.Count));
				}
				catch (WebSocketException wex)
				{
					ErrorReceiving?.Invoke(wex);
					Close();
					break;
				}

				if (!result.EndOfMessage)
					continue;
				Debug.WriteLine(msg.ToString());
				handlerQueue.Add(msg.ToString());
				msg = new StringBuilder();
			}
		}

		private void HandleItem(string data)
		{
			// deserialize to Messge
			SlackSocketMessage message;
			try
			{
				message = JsonConvert.DeserializeObject<SlackSocketMessage>(data, new UnixDateTimeConverter());
			}
			catch (JsonSerializationException)
			{
				return;
			}

			if (message == null)
				return;

			// check Slack says ok
			if (!message.ok)
			{
				if (message.error == null)
					Debug.WriteLine($"ERROR: data");
				else
					Debug.WriteLine($"ERROR: {message.error.code} - {message.error.msg} = {data}");
				return;
			}

			// is it a callback?
			Action<string> callback;
			if (message.reply_to != 0 && callbacks.TryGetValue(message.reply_to, out callback))
			{
				try
				{
					callback(data);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Callback failed: " + ex.Message);
				}
				return;
			}

			// handle Message
			try
			{
				if (string.IsNullOrEmpty(message.type))
				{
					if (message.reply_to == 0)
					{
						Debug.WriteLine("Message with no Type and not reply_to: " + data);
						return;
					}

					message.type = "reply_to";
				}
				SlackEventHandler handler;
				if (handlers.TryGetValue(message.type, out handler))
					handler.Handle(this, data);
				else
					Debug.WriteLine("No handler for " + message.type);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Handler failed: " + ex.Message);
			}
		}

		private void SendItem(string message)
		{
			if (cts.Token.IsCancellationRequested)
				return;

			byte[] sending = Encoding.UTF8.GetBytes(message);
			var buffer = new ArraySegment<byte>(sending);
			try
			{
				socket.SendAsync(buffer, WebSocketMessageType.Text, true, cts.Token).Wait();
			}
			catch (WebSocketException wex)
			{
				ErrorSending?.Invoke(wex);
				Close();
			}
		}

		public void Close()
		{
			try
			{
				cts.Cancel();
				socket.Abort();
			}
			catch (Exception)
			{
				// ignored
			}

			if (Interlocked.CompareExchange(ref closedEmitted, 1, 0) == 0)
				ConnectionClosed?.Invoke();
		}

		private class SlackEventHandler
		{
			public SlackEventHandler(Type handlerType)
			{
				var handlerIf = handlerType
					.GetInterfaces()
					.Where(i => i.IsGenericType)
					.FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

				if (handlerIf == null)
					throw new ArgumentException("handlerType does not implement IEventHandler<T>");

				messageType = handlerIf.GetGenericArguments()[0];
				handlerMethod = handlerType.GetMethod("Handle", new[] { typeof(ISlackSocket), messageType });
				handler = Activator.CreateInstance(handlerType);
			}

			private readonly object handler;
			private readonly Type messageType;
			private readonly MethodInfo handlerMethod;

			public void Handle(ISlackSocket socket, string data)
			{
				object message = JsonConvert.DeserializeObject(data, messageType, new UnixDateTimeConverter());
				handlerMethod.Invoke(handler, new[] { socket, message });
			}
		}
	}

	public class SlackSocketMessage
	{
		public bool ok = true;
		public Error error;

		public int id;
		public int reply_to;

		public string type;
	}

	public class Error
	{
		public int code;
		public string msg;
	}
}
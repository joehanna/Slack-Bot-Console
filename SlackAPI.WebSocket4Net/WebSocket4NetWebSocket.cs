using System;
using System.Diagnostics;
using System.Threading.Tasks;

using WebSocket4Net;

using Pook.SlackAPI;

namespace SlackAPI.WebSocket4Net
{
	public class WebSocket4NetWebSocket : IWebSocket
	{
		private WebSocket socket;

		public bool IsConnected { get; private set; }
		public event Action<string> MessageReceived;

		public Task ConnectAsync(Uri uri)
		{
			socket = new WebSocket(uri.ToString());
			socket.Opened += (sender, args) => IsConnected = true;
			socket.Error += (sender, args) => { Debug.WriteLine("ERROR: " + args.Exception.ToString()); };
			socket.Closed += (sender, args) => IsConnected = false;
			socket.MessageReceived += (sender, args) => MessageReceived?.Invoke(args.Message);
			socket.Open();

			return Task.FromResult(0);
		}

		public Task SendAsync(string message)
		{
			socket.Send(message);
			return Task.FromResult(0);
		}

		public void Abort()
		{
			socket.Close();
		}
	}
}
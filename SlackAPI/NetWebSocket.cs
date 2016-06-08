using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pook.SlackAPI
{
    public class NetWebSocket : IWebSocket
    {
        private readonly ClientWebSocket socket;
        private readonly CancellationToken cancellationToken;

        public event Action<string> MessageReceived;

        public NetWebSocket(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            socket = new ClientWebSocket();
        }

        public WebSocketState State => socket.State;

        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;

        public void Abort()
        {
            socket.Abort();
        }

        public async Task ConnectAsync(Uri uri)
        {
            await socket.ConnectAsync(uri, cancellationToken);

            await Task.Factory
                .StartNew(async () => await SocketLoop(), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        Trace.TraceError(t.Exception.Message);
                    Trace.TraceInformation("NetWebSocket.SocketLoop finished");
                }, CancellationToken.None);
        }

        private async Task SocketLoop()
        {
            while (IsConnected)
            {
                var msg = await ReceiveAsync();
                MessageReceived?.Invoke(msg);
            }
        }

        public async Task<string> ReceiveAsync()
        {
            WebSocketReceiveResult result = null;
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var msg = new StringBuilder();
            while (result == null || !result.EndOfMessage)
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                msg.Append(Encoding.UTF8.GetString(buffer.Array, 0, result.Count));
            }

            return msg.ToString();
        }

        public Task SendAsync(string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            return socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}
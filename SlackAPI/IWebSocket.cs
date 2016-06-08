using System;
using System.Threading.Tasks;

namespace Pook.SlackAPI
{
    public interface IWebSocket
    {
        bool IsConnected { get; }

        event Action<string> MessageReceived;

        Task ConnectAsync(Uri uri);
        Task SendAsync(string message);
        void Abort();
    }
}
using System.Diagnostics;

namespace Pook.SlackAPI.RTMHandlers
{
    [SlackSocketRouting("reconnect_url")]
    public class ReconnectUrlHandler : IEventHandler<ReconnectUrl>
    {
        public void Handle(ISlackSocket socket, ReconnectUrl message)
        {
            Debug.WriteLine($"reconnect to {message.url}");
        }
    }
}
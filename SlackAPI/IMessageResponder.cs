using System;
using System.Threading.Tasks;
using Pook.SlackAPI.RTMMessages;

namespace Pook.SlackAPI
{
	public interface IMessageResponder
	{
		bool CanRespond(Message message, SlackUser user);
		Task Respond(ISlackSocket socket, Message message, SlackUser user);
	}
}
using System;
using System.Threading.Tasks;

using Pook.SlackAPI;
using Pook.SlackAPI.RTMMessages;

namespace SlackConsole
{
	/// <summary>
	/// Not current used. Requires Command prompt host library (not available).
	/// Shows usage for some of the API
	/// </summary>
	public class Commands
	{
		private SlackSocket socket;
		
		public async Task Login(string token)
		{
			if (string.IsNullOrEmpty(token))
				throw new Exception("a Slack RTM token MUST be supplied");

			socket = new SlackSocket(token);
			await socket.Login();
			Console.WriteLine(socket.State.Url);
			Console.Write("Press enter to quit...");
			Console.ReadLine();
		}

		public void RtmSend(string message)
		{
			var msg = new Message
			{
				user = socket.State.Self.id,
				text = message
			};

			socket.Send(msg);
		}

		public async Task ApiSend(string channel, string message)
		{
			var response = await socket.API.ChatPostMessage(channel, message);
			Console.WriteLine($"ok={response.ok} {response.message}");
		}

		public async Task HookSend(string message, string channel)
		{
			var client = new SlackWebHookClient("A SLACK INCOMING WEBHOOK URL");
			await client.SendMessage(message, channel);
		}
	}
}
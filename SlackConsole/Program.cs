using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pook.SlackAPI;
using Pook.SlackAPI.RTMMessages;

namespace SlackConsole
{
	/// <summary>
	/// Very simple host for Slack bot.
	/// <para>
	/// Invoke from a command line with: slackconsole.exe &lt;bot token&gt;
	/// </para>
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("A Slack token MUST be provided");
				return;
			}

			var token = args[0];
			if (string.IsNullOrEmpty(token))
				throw new Exception("a Slack RTM token MUST be supplied");

            Trace.Listeners.Add(new ConsoleTraceListener());

			var socket = new SlackSocket(token).AddBuiltinEventHandlers().AddBuiltinResponders();
			socket.Login().Wait();
			Console.WriteLine(socket.State.Url);

            Console.Write("Press enter to quit...");
			Console.ReadLine();
		}
	}

}
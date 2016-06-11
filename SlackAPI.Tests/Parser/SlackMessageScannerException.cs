using System;

namespace SlackAPI.Tests.Parser
{
	public class SlackMessageScannerException : Exception
	{
		public SlackMessageScannerException(string message) : base(message) { }
		public SlackMessageScannerException(string message, params object[] args) : base(string.Format(message, args)) { }
	}
}
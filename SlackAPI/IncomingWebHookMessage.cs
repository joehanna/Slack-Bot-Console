using System.Diagnostics.CodeAnalysis;

namespace Pook.SlackAPI
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class IncomingWebHookMessage
	{
		public string channel { get; set; }
		public string icon_emoji { get; set; }
		public string icon_url { get; set; }
		public string text { get; set; }
		public bool unfurl_links { get; set; }
		public string username { get; set; }
	}
}

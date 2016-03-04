// ReSharper disable InconsistentNaming
namespace Pook.SlackAPI.APIMessages
{
	public class Message : SlackSocketMessage
	{
		public string channel;
		public string ts;
		public string user;
		/// <summary>
		/// Isn't always set. Should look up if not set.
		/// </summary>
		public string username;
		public string text;
		public bool is_starred;
		public string permalink;
	}
}
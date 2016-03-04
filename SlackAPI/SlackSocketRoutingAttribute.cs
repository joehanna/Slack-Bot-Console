using System;

namespace Pook.SlackAPI
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class SlackSocketRoutingAttribute : Attribute
	{
		public SlackSocketRoutingAttribute(string type)
		{
			Type = type;
		}

		public string Type { get; }
	}
}
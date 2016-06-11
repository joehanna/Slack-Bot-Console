using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SlackAPI.Tests.Parser
{
	public class SlackMessageParseResult : IEnumerable<Node>
	{
		private readonly List<Node> nodes = new List<Node>();

		public int Count => nodes.Count;

		public Node this[int i] => nodes[i];

		public void Add(Node node)
		{
			nodes.Add(node);
		}

		public IEnumerator<Node> GetEnumerator()
		{
			return nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var node in nodes)
				sb.Append($"{node.NodeType.Name}[{node}]");

			return sb.ToString();
		}
	}

	public abstract class Node
	{
		private Type type;
		public Type NodeType { get { return type ?? (type = GetType()); } }
	}

	public class SlackID : Node
	{
		public enum IDType
		{
			ID,
			Command,
			Uri
		}

		public IDType Type { get; set; }
		public string ID { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{
			char? type = null;
			switch (Type)
			{
				case IDType.ID:
					type = '@';
					break;
				case IDType.Command:
					type = '!';
					break;
			}

			return $"<{type}{ID}|{Name}>";
		}
	}

	public class ChannelNode : Node
	{
		public string Name { get; set; }
		public override string ToString()
		{
			return $"#{Name}";
		}
	}
	public class WordNode : Node
	{
		public string Text { get; set; }
		public override string ToString()
		{
			return Text;
		}
	}
	public class SlugNode : Node
	{
		public string Text { get; set; }
		public override string ToString()
		{
			return "{" + Text + "}";
		}
	}
}
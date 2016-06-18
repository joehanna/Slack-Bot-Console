using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SlackAPI.Parser
{
	public class SlackMessageMatcher
	{

	}

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
}
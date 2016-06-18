using System;

namespace SlackAPI.Parser
{
	public class SlackMessageParser
	{
		public static SlackMessageParseResult Parse(string text)
		{
			return new SlackMessageParser(text).Parse();
		}

		public SlackMessageParser(string text)
		{
			scanner = new SlackMessageScanner(text);
		}

		private SlackMessageScanner scanner;
		public SlackMessageParseResult Result { get; } = new SlackMessageParseResult();

		public SlackMessageParseResult Parse()
		{
			scanner.Next();
			do
			{
				switch (scanner.Token)
				{
					case SlackMessageScanner.Tokens.LANGLE:
						ReadSlackID();
						break;
					case SlackMessageScanner.Tokens.CHAR:
					case SlackMessageScanner.Tokens.DIGIT:
						ReadWord();
						break;
					case SlackMessageScanner.Tokens.HASH:
						ReadChannel();
						break;
					case SlackMessageScanner.Tokens.LBRACE:
						ReadSlug();
						break;
				}
				scanner.SkipWhiteSpace();
			} while (scanner.Token != SlackMessageScanner.Tokens.EOF);
			return Result;
		}

		private void ReadWord()
		{
			Result.Add(new WordNode
			{
				Text = scanner.ReadWord()
			});
		}
		private void ReadChannel()
		{
			Result.Add(new ChannelNode
			{
				Name = scanner.ReadWord()
			});
		}
		private void ReadSlug()
		{
			scanner.Next();
			Result.Add(new SlugNode
			{
				Text = scanner.ReadTo(SlackMessageScanner.Tokens.RBRACE)
			});
			scanner.Next();
		}
		private void ReadSlackID()
		{
			var node = new SlackID();
			scanner.Next();
			switch (scanner.Token)
			{
				case SlackMessageScanner.Tokens.AT:
					node.Type = SlackID.IDType.ID;
					break;
				case SlackMessageScanner.Tokens.BANG:
					node.Type = SlackID.IDType.ID;
					break;
				default:
					node.Type = SlackID.IDType.Uri;
					break;
			}
			scanner.Next();
			node.ID = scanner.ReadToAny(SlackMessageScanner.Tokens.PIPE, SlackMessageScanner.Tokens.RANGLE);
			if (scanner.Token == SlackMessageScanner.Tokens.PIPE)
			{
				scanner.Next();
				node.Name = scanner.ReadToAny(SlackMessageScanner.Tokens.RANGLE);
			}
			scanner.Next();
			Result.Add(node);
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
		public SlackMessageSpan ID { get; set; }
		public SlackMessageSpan Name { get; set; }

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
		public SlackMessageSpan Name { get; set; }
		public override string ToString()
		{
			return $"#{Name.ToString()}";
		}
	}
	public class WordNode : Node
	{
		public SlackMessageSpan Text { get; set; }
		public override string ToString()
		{
			return Text.ToString();
		}
	}
	public class SlugNode : Node
	{
		public SlackMessageSpan Text { get; set; }
		public override string ToString()
		{
			return "{" + Text.ToString() + "}";
		}
	}
}
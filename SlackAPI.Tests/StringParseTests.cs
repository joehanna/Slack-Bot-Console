using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace SlackAPI.Tests
{
	public class StringParseTests
	{
		[Fact]
		public void Simple()
		{
			var result = "login fred".ParseExact("login {0}");
			Assert.True(result[0] == "fred");
		}

		[Fact]
		public void ManyParts()
		{
			var result = "login fred with me on host".ParseExact("login {0} with {1} on {2}");
			Assert.Equal("fred", result[0]);
			Assert.Equal("me", result[1]);
			Assert.Equal("host", result[2]);
		}

		[Fact]
		public void Parse()
		{
			var text = "<@U123|fred> hello";
			var result = new MsgParser(text).Parse();

			Assert.Equal(2, result.Count);
			Assert.Equal("U123", ((MsgParser.SlackID)result[0]).User);
			Assert.Equal("fred", ((MsgParser.SlackID)result[0]).Name);
			Assert.Equal("hello", ((MsgParser.WordNode)result[1]).Text);
		}

		[Fact]
		public void CanRecognizeTokens()
		{
			const string input = "<@U1|f>";
			var expectedTokens = new[] {
				Scanner.CharToken.LAngle,
				Scanner.CharToken.At,
				Scanner.CharToken.Char,
				Scanner.CharToken.Digit,
				Scanner.CharToken.Pipe,
				Scanner.CharToken.Char,
				Scanner.CharToken.RAngle
			};
			var scanner = new Scanner(input);
			foreach (var expectedToken in expectedTokens)
			{
				scanner.Next();
				Assert.Equal(expectedToken, scanner.Token);
			}
		}
	}

	//public class SlackMessageParser
	//{
	//	public Result Parse(string text)
	//	{

	//	}

	//	private char Next()
	//	{

	//	}

	//	public class Result
	//	{

	//	}
	//}

	public class MsgParser
	{
		public MsgParser(string text)
		{
			scanner = new Scanner(text);
		}

		private Scanner scanner;
		public ParseResult Result { get; } = new ParseResult();

		public ParseResult Parse()
		{
			do
			{
				scanner.Next();
				switch (scanner.Token)
				{
					case Scanner.CharToken.LAngle:
						ReadSlackID();
						break;
					case Scanner.CharToken.Char:
						ReadWord();
						break;
				}
			} while (scanner.Token != Scanner.CharToken.EOF);
			return Result;
		}

		private void ReadWord()
		{
			Result.Add(new WordNode
			{
				Text = scanner.ReadWord()
			});
		}

		private void ReadSlackID()
		{
			var node = new SlackID();
			scanner.Next();
			if (scanner.Token != Scanner.CharToken.At)
				throw new Exception();
			scanner.Next();
			node.User = scanner.ReadToAny(Scanner.CharToken.Pipe, Scanner.CharToken.RAngle);
			if (scanner.Token == Scanner.CharToken.Pipe)
			{
				scanner.Next();
				node.Name = scanner.ReadToAny(Scanner.CharToken.RAngle);
			}
			Result.Add(node);
		}

		public class ParseResult : IEnumerable<Node>
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
		}

		public abstract class Node { }
		public class SlackID : Node
		{
			public string User { get; set; }
			public string Name { get; set; }
		}
		public class WordNode : Node
		{
			public string Text { get; set; }
		}
	}

	public class Scanner
	{
		public enum CharToken
		{
			Char,
			Digit,
			WS,
			LAngle,
			RAngle,
			Pipe,
			At,
			EOF
		}

		public Scanner(string source)
		{
			text = source;
		}

		private readonly string text;
		private int pos = 0;

		private char ch;
		public CharToken Token { get; private set; }
		public int Position => pos;

		public string ReadToAny(params CharToken[] tokens)
		{
			var start = pos - 1;
			do
			{
				Next();
			} while (!tokens.Contains(Token));
			return text.Substring(start, pos - start - 1);
		}

		public string ReadWord()
		{
			var start = pos - 1;
			do
			{
				Next();
			} while (Token == CharToken.Char || Token == CharToken.Digit);
			var end = pos;
			SkipWhiteSpace();
			return text.Substring(start, end - start);
		}

		public void SkipWhiteSpace()
		{
			while (Token == CharToken.WS)
				Next();
		}

		public void Next()
		{
			var r = Read();
			if (r < 0)
			{
				Token = CharToken.EOF;
				return;
			}

			var ch = (char)r;
			if (char.IsDigit(ch))
				Token = CharToken.Digit;
			else if (char.IsLetter(ch))
				Token = CharToken.Char;
			else if (char.IsWhiteSpace(ch))
				Token = CharToken.WS;
			else {
				switch (ch)
				{
					case '@':
						Token = CharToken.At;
						break;
					case '|':
						Token = CharToken.Pipe;
						break;
					case '<':
						Token = CharToken.LAngle;
						break;
					case '>':
						Token = CharToken.RAngle;
						break;
				}
			}
		}

		private int Read()
		{
			if (pos >= text.Length)
				return -1;

			return text[pos++];
		}
	}
}

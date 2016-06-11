using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xunit;

using SlackAPI;
using System.Collections.Generic;
using System.Collections;

namespace SlackAPI.Tests
{
	public class ScannerTests
	{
		[Fact]
		public void Parse()
		{
			var text = "<@U123|fred> hello";
			var result = new SlackMessageParser(text).Parse();

			Assert.Equal(2, result.Count);

			Assert.Equal(typeof(SlackMessageParser.SlackID), result[0].Type);
			var id = (SlackMessageParser.SlackID)result[0];
			Assert.Equal("U123", id.User);
			Assert.Equal("fred", id.Name);

			Assert.Equal(typeof(SlackMessageParser.WordNode), result[1].Type);
			var word = (SlackMessageParser.WordNode)result[1];
			Assert.Equal("hello", word.Text);
		}

		[Fact]
		public void CanRecognizeTokens()
		{
			const string input = "<@U1|f>";
			var expectedTokens = new[] {
				SlackMessageScanner.Tokens.LANGLE,
				SlackMessageScanner.Tokens.AT,
				SlackMessageScanner.Tokens.CHAR,
				SlackMessageScanner.Tokens.DIGIT,
				SlackMessageScanner.Tokens.PIPE,
				SlackMessageScanner.Tokens.CHAR,
				SlackMessageScanner.Tokens.RANGLE
			};
			var scanner = new SlackMessageScanner(input);
			foreach (var expectedToken in expectedTokens)
			{
				scanner.Next();
				Assert.Equal(expectedToken, scanner.Token);
			}
		}
	}

	public class SlackMessageParser
	{
		public SlackMessageParser(string text)
		{
			scanner = new SlackMessageScanner(text);
		}

		private SlackMessageScanner scanner;
		public ParseResult Result { get; } = new ParseResult();

		public ParseResult Parse()
		{
			do
			{
				scanner.Next();
				switch (scanner.Token)
				{
					case SlackMessageScanner.Tokens.LANGLE:
						ReadSlackID();
						break;
					case SlackMessageScanner.Tokens.CHAR:
						ReadWord();
						break;
				}
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

		private void ReadSlackID()
		{
			var node = new SlackID();
			scanner.Next();
			if (scanner.Token != SlackMessageScanner.Tokens.AT)
				throw new Exception();
			scanner.Next();
			node.User = scanner.ReadToAny(SlackMessageScanner.Tokens.PIPE, SlackMessageScanner.Tokens.RANGLE);
			if (scanner.Token == SlackMessageScanner.Tokens.PIPE)
			{
				scanner.Next();
				node.Name = scanner.ReadToAny(SlackMessageScanner.Tokens.RANGLE);
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

		public abstract class Node
		{
			public Node()
			{
				Type = GetType();
			}

			public Type Type { get; }
		}
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

	public class SlackMessageScanner
	{
		public enum Tokens
		{
			CHAR,
			DIGIT,
			WS,
			LANGLE,
			RANGLE,
			PIPE,
			AT,
			EOF
		}

		public SlackMessageScanner(string source)
		{
			text = source;
		}

		private readonly string text;
		private int pos = 0;

		private char ch;
		public Tokens Token { get; private set; }
		public int Position => pos;

		public string ReadToAny(params Tokens[] tokens)
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
			} while (Token == Tokens.CHAR || Token == Tokens.DIGIT);
			var end = pos;
			SkipWhiteSpace();
			return text.Substring(start, end - start);
		}

		public void SkipWhiteSpace()
		{
			while (Token == Tokens.WS)
				Next();
		}

		public void Next()
		{
			var r = Read();
			if (r < 0)
			{
				Token = Tokens.EOF;
				return;
			}

			var ch = (char)r;
			if (char.IsDigit(ch))
				Token = Tokens.DIGIT;
			else if (char.IsLetter(ch))
				Token = Tokens.CHAR;
			else if (char.IsWhiteSpace(ch))
				Token = Tokens.WS;
			else {
				switch (ch)
				{
					case '@':
						Token = Tokens.AT;
						break;
					case '|':
						Token = Tokens.PIPE;
						break;
					case '<':
						Token = Tokens.LANGLE;
						break;
					case '>':
						Token = Tokens.RANGLE;
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

		private void LexicalError()
		{
			throw new SlackMessageScannerException("Lexical error at pos={0} ch='{1}'", pos, ch);
		}
	}

	public class SlackMessageScannerException : Exception
	{
		public SlackMessageScannerException(string message) : base(message) { }
		public SlackMessageScannerException(string message, params object[] args) : base(string.Format(message, args)) { }
	}
}
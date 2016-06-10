using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xunit;

using SlackAPI;

namespace SlackAPI.Tests
{
	public class ScannerTests
	{
		[Fact]
		public void CanRecognizeTokens()
		{
			const string input = "REPEAT 4 [ FORWARD 100 LEFT 90 ] ";
			var expectedTokens = new[] {
				Token.REPEAT, Token.NUMBER, Token.LBRACKET,
				Token.FORWARD, Token.NUMBER, Token.LEFT,
				Token.NUMBER, Token.RBRACKET, Token.EOF
			};
			var scanner = new SlackMessageScanner(input);
			foreach (var expectedToken in expectedTokens)
			{
				var token = scanner.Scan();
				Assert.Equal(expectedToken, token);
			}
		}
	}

	public enum Token
	{
		[TokenAsText("REPEAT")]
		REPEAT,
		[TokenAsText("FORWARD")]
		FORWARD,
		[TokenAsText("BACK")]
		BACK,
		[TokenAsText("LEFT")]
		LEFT,
		[TokenAsText("RIGHT")]
		RIGHT,
		[TokenAsText("NUMBER")]
		NUMBER,
		[TokenAsText("[")]
		LBRACKET,
		[TokenAsText("]")]
		RBRACKET,
		EOF,
		NONE
	}
	public class TokenAsTextAttribute : Attribute
	{
		public TokenAsTextAttribute(string text)
		{
			Text = text;
		}

		public string Text { get; }
	}
	public static class TokenHelper
	{
		public static Token TextToToken(string s)
		{
			Type tokenType = typeof(Token);
			MemberInfo[] fields = tokenType.GetMembers(BindingFlags.Public | BindingFlags.Static);
			foreach (MemberInfo memberInfo in fields)
			{
				object[] attr = memberInfo.GetCustomAttributes(typeof(TokenAsTextAttribute), false);
				if (attr.Length > 0)
				{
					string text = ((TokenAsTextAttribute)attr[0]).Text;
					if (text.Equals(s))
					{
						return (Token)Enum.Parse(tokenType, memberInfo.Name);
					}
				}
			}
			return Token.NONE;
		}

		public static string TokenToText(Token token)
		{
			Type tokenType = typeof(Token);
			MemberInfo[] field = tokenType.GetMember(Enum.GetName(tokenType, token));
			object[] attr = field[0].GetCustomAttributes(typeof(TokenAsTextAttribute), false);
			if (attr.Length > 0)
			{
				return ((TokenAsTextAttribute)attr[0]).Text;
			}
			return token.ToString();
		}
	}

	public class SlackMessageScanner
	{
		readonly Token[] reserved = new[] { Token.FORWARD, Token.BACK, Token.RIGHT, Token.LEFT, Token.REPEAT };

		private readonly string rawContents;
		private string scanBuffer;
		private int idx;
		private char ch;

		public SlackMessageScanner(string input)
		{
			rawContents = input;
		}

		public Token Scan()
		{
			while (idx < rawContents.Length)
			{
				ch = rawContents[idx];
				if (ch == '[')
				{
					idx++;
					return Token.LBRACKET;
				}
				else if (ch == ']')
				{
					idx++;
					return Token.RBRACKET;
				}
				else if (char.IsDigit(ch))
				{
					scanBuffer = ch.ToString();
					idx++;
					while (idx < rawContents.Length)
					{
						ch = rawContents[idx];
						if (char.IsDigit(ch))
						{
							scanBuffer += ch;
							idx++;
						}
						else break;
					}
					return Token.NUMBER;
				}
				else if (char.IsLetter(ch))
				{
					scanBuffer = ch.ToString();
					idx++;
					while (idx < rawContents.Length)
					{
						ch = rawContents[idx];
						if (char.IsLetter(ch))
						{
							scanBuffer += ch;
							idx++;
						}
						else break;
					}
					Token lookup;
					if (LookupReserved(scanBuffer, out lookup))
						return lookup;

					LexicalError();
				}
				else if (char.IsWhiteSpace(ch))
					idx++;
				else
					LexicalError();
			}
			return Token.EOF;
		}

		public Token NextToken()
		{
			var oldIdx = idx;
			var result = Scan();
			idx = oldIdx;
			return result;
		}

		private void LexicalError()
		{
			throw new ScannerException("Lexical error at '{0}'('{1}')", ch, scanBuffer);
		}

		private bool LookupReserved(string s, out Token lookup)
		{
			lookup = TokenHelper.TextToToken(s);
			return reserved.Contains(lookup);
		}
	}

	public class ScannerException : Exception
	{
		public ScannerException(string message) : base(message) { }
		public ScannerException(string message, params object[] args) : base(string.Format(message, args)) { }
	}
}
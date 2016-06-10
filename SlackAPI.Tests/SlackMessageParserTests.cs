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
			const string input = "<@U123|fred>";
			var expectedTokens = new[] { Token.LANGLE, Token.AT, Token.WORD, Token.NUMBER, Token.PIPE, Token.WORD, Token.RANGLE };
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
		AT,
		LANGLE,
		RANGLE,
		PIPE,
		WORD,
		NUMBER,
		EOF,
		NONE
	}

	public class SlackMessageScanner
	{
		private readonly string text;
		private int idx;
		private string word;
		private char ch;

		public SlackMessageScanner(string input)
		{
			text = input;
		}

		public Token Scan()
		{
			while (idx < text.Length)
			{
				ch = text[idx];
				if (ch == '<')
				{
					idx++;
					return Token.LANGLE;
				}
				else if (ch == '>')
				{
					idx++;
					return Token.RANGLE;
				}
				else if (ch == '|')
				{
					idx++;
					return Token.PIPE;
				}
				else if (ch == '@')
				{
					idx++;
					return Token.AT;
				}
				else if (char.IsDigit(ch))
				{
					word = ch.ToString();
					idx++;
					while (idx < text.Length)
					{
						ch = text[idx];
						if (char.IsDigit(ch))
						{
							word += ch;
							idx++;
						}
						else break;
					}
					return Token.NUMBER;
				}
				else if (char.IsLetter(ch))
				{
					word = ch.ToString();
					idx++;
					while (idx < text.Length)
					{
						ch = text[idx];
						if (char.IsLetter(ch))
						{
							word += ch;
							idx++;
						}
						else break;
					}

					return Token.WORD;
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
			throw new SlackMessageScannerException("Lexical error at '{0}'('{1}')", ch, word);
		}
	}

	public class SlackMessageScannerException : Exception
	{
		public SlackMessageScannerException(string message) : base(message) { }
		public SlackMessageScannerException(string message, params object[] args) : base(string.Format(message, args)) { }
	}
}
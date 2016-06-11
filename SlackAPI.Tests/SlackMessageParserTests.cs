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
			var expectedTokens = new[] { SlackMessageToken.LANGLE, SlackMessageToken.AT, SlackMessageToken.WORD, SlackMessageToken.NUMBER, SlackMessageToken.PIPE, SlackMessageToken.WORD, SlackMessageToken.RANGLE };
			var scanner = new SlackMessageScanner(input);
			foreach (var expectedToken in expectedTokens)
			{
				var token = scanner.Scan();
				Assert.Equal(expectedToken, token);
			}
		}
	}

	public enum SlackMessageToken
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

		public SlackMessageToken Scan()
		{
			while (idx < text.Length)
			{
				ch = text[idx];
				if (ch == '<')
				{
					idx++;
					return SlackMessageToken.LANGLE;
				}
				else if (ch == '>')
				{
					idx++;
					return SlackMessageToken.RANGLE;
				}
				else if (ch == '|')
				{
					idx++;
					return SlackMessageToken.PIPE;
				}
				else if (ch == '@')
				{
					idx++;
					return SlackMessageToken.AT;
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
					return SlackMessageToken.NUMBER;
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

					return SlackMessageToken.WORD;
				}
				else if (char.IsWhiteSpace(ch))
					idx++;
				else
					LexicalError();
			}
			return SlackMessageToken.EOF;
		}

		public SlackMessageToken NextToken()
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
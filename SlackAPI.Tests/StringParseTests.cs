using System;
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

	class Scanner : StringReader
	{
		string currentWord;

		public Scanner(string source) : base(source)
		{
			readNextWord();
		}

		private void readNextWord()
		{
			var sb = new StringBuilder();
			char nextChar;
			int next;
			do
			{
				next = Read();
				if (next < 0)
					break;
				nextChar = (char)next;
				if (char.IsWhiteSpace(nextChar))
					break;
				sb.Append(nextChar);
			} while (true);
			while (Peek() >= 0 && char.IsWhiteSpace((char)Peek()))
				Read();
			if (sb.Length > 0)
				currentWord = sb.ToString();
			else
				currentWord = null;
		}
	}
}

using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xunit;

using SlackAPI.Tests.Parser;
using System.Diagnostics;
using Xunit.Abstractions;

namespace SlackAPI.Tests
{
	public class ScannerTests
	{
		private readonly ITestOutputHelper output;

		public ScannerTests(ITestOutputHelper output)
		{
			this.output = output;
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

		[Fact]
		public void Parse()
		{
			var text = "<@U123|fred> hello";
			var result = SlackMessageParser.Parse(text);

			Assert.Equal(2, result.Count);

			Assert.Equal(typeof(SlackID), result[0].NodeType);
			var id = (SlackID)result[0];
			Assert.Equal(id.Type, SlackID.IDType.ID);
			Assert.Equal("U123", id.ID);
			Assert.Equal("fred", id.Name);

			Assert.Equal(typeof(WordNode), result[1].NodeType);
			var word = (WordNode)result[1];
			Assert.Equal("hello", word.Text);
		}

		[Fact]
		public void Parse_complex()
		{
			var text = "<@U123|fred> login as {user}";
			var result = new SlackMessageParser(text).Parse();

			output.WriteLine(result.ToString());
		}
	}
}
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xunit;

using SlackAPI;
using System.Collections.Generic;
using System.Collections;

namespace SlackAPI.Tests.Parser
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
}
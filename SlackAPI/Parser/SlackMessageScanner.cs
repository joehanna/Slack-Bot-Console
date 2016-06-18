using System;
using System.Linq;

namespace SlackAPI.Parser
{
	public class SlackMessageScanner
	{
		public enum Tokens
		{
			CHAR,
			DIGIT,
			WS,
			LANGLE,
			RANGLE,
			LBRACE,
			RBRACE,
			PIPE,
			AT,
			STAR,
			AMP,
			BANG,
			HASH,
			DOLLAR,
			EOF
		}

		public SlackMessageScanner(string text)
		{
			this.text = text;
		}

		private readonly string text;
		private int pos = -1;

		private char ch;
		public Tokens Token { get; private set; }
		public int Position => pos;

		public SlackMessageSpan ReadTo(Tokens token)
		{
			var start = pos;
			do
			{
				Next();
			} while (Token != token);
			return new SlackMessageSpan(text, start, pos - start);
		}
		public SlackMessageSpan ReadToAny(params Tokens[] tokens)
		{
			var start = pos;
			do
			{
				Next();
			} while (!tokens.Contains(Token));
			return new SlackMessageSpan(text, start, pos - start);
		}

		public SlackMessageSpan ReadWord()
		{
			if (Token != Tokens.CHAR && Token != Tokens.DIGIT)
				return SlackMessageSpan.Empty;

			var start = pos;
			do
			{
				Next();
			} while (Token == Tokens.CHAR || Token == Tokens.DIGIT);

			return new SlackMessageSpan(text, start, pos - start);
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
				ch = '\0';
				Token = Tokens.EOF;
				return;
			}

			ch = (char)r;
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
				case '{':
					Token = Tokens.LBRACE;
					break;
				case '}':
					Token = Tokens.RBRACE;
					break;
				case '!':
					Token = Tokens.BANG;
					break;
				case '&':
					Token = Tokens.AMP;
					break;
				case '#':
					Token = Tokens.HASH;
					break;
				case '$':
					Token = Tokens.DOLLAR;
					break;
				default:
					if (char.IsDigit(ch))
						Token = Tokens.DIGIT;
					else if (char.IsLetter(ch))
						Token = Tokens.CHAR;
					else if (char.IsWhiteSpace(ch))
						Token = Tokens.WS;
					break;
			}
		}
		private int Read()
		{
			if (++pos >= text.Length)
				return -1;

			var x = text[pos];
			ch = x;
			return x;
		}

		private void LexicalError()
		{
			throw new SlackMessageScannerException("Lexical error at pos={0} ch='{1}'", pos, ch);
		}
	}
}
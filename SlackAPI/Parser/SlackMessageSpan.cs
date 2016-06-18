
namespace SlackAPI.Parser
{
	public struct SlackMessageSpan
	{
		public static readonly SlackMessageSpan Empty = new SlackMessageSpan(string.Empty, 0, 0);

		public SlackMessageSpan(string text, int start, int length)
		{
			this.text = text;
			Start = start;
			Length = length;
			End = start + length;
		}

		private readonly string text;

		public int Start { get; }
		public int End { get; }
		public int Length { get; }

		public bool Equals(string match)
		{
			if (Length != match.Length)
				return false;
			int x = 0;
			for (int i = Start; i < End; i++, x++)
			{
				if (text[i] != match[x])
					return false;
			}
			return true;
		}
		public bool StartsWith(string match)
		{
			if (Length < match.Length)
				return false;
			int x = 0;
			for (int i = Start; i < match.Length; i++, x++)
			{
				if (text[i] != match[x])
					return false;
			}
			return true;
		}

		public override string ToString()
		{
			return text.Substring(Start, Length);
		}

		public static implicit operator string(SlackMessageSpan span)
		{
			return span.ToString();
		}
	}
}
using SlackAPI.Parser;
using Xunit;

namespace SlackAPI.Tests
{
	public class SpanTests
	{
		private string text = "hello world";

		[Fact]
		public void Test()
		{
			var span = new SlackMessageSpan(text, 0, 5);

			Assert.Equal("hello", span.ToString());
			Assert.Equal("hello", span);
			Assert.True(span.Equals("hello"));
			Assert.True(span.StartsWith("hello"));
			Assert.True(span.StartsWith("hel"));
		}
	}
}
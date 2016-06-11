using System;
using System.Collections;
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
}

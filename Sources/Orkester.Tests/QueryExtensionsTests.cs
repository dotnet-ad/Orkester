using NUnit.Framework;
using System;
namespace Orkester.Tests
{
	[TestFixture()]
	public class QueryExtensionsTests
	{
		static readonly string[] validQueries = new[] 
		{ 
			"a/b/c?p1=a&p2=b", 
			"a/b/c?", 
			"?p1=a",
		};

		[Test()]
		public void ShouldExtractPathFromValidQueries()
		{
			var expectedPaths = new[] { "a/b/c", "a/b/c", "" };

			for (int i = 0; i < validQueries.Length; i++)
			{
				var path = validQueries[i].ExtractQueryPath();
				Assert.AreEqual(expectedPaths[i], path);
			}
		}

		[Test()]
		public void ShouldExtractQueryStringFromValidQueries()
		{
			var expectedQueryStrings = new[] { "?p1=a&p2=b", "?", "?p1=a" };

			for (int i = 0; i < validQueries.Length; i++)
			{
				var queryString = validQueries[i].ExtractQueryString();
				Assert.AreEqual(expectedQueryStrings[i], queryString);
			}
		}

		[Test()]
		public void ShouldExtractQueryParametersFromValidQueries()
		{
			var q1 = "?p1=a&p2=b";

			var dict = q1.ToQueryDictionnary();

			Assert.IsTrue(dict.ContainsKey("p1"));
			Assert.IsTrue(dict.ContainsKey("p2"));

			Assert.AreEqual("a", dict["p1"]);
			Assert.AreEqual("b", dict["p2"]);
		}

		[Test()]
		public void ShouldExtractOrdersQueryParameters()
		{
			var q1 = "?p3=a&p1=b&p2=c";

			var ordered = q1.ToOrderedQueryString();

			Assert.AreEqual("?p1=b&p2=c&p3=a", ordered);
		}

		// TODO Should test uri encoding too
	}
}
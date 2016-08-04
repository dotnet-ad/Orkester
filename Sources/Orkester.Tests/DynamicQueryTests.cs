namespace Orkester.Tests
{
	using NUnit.Framework;

	[TestFixture()]
	public class DynamicQueryTests 
	{
		[Test()]
		public void ShouldExtractParametersFromValidQueries()
		{
			const string query = "a/b/c?p1=a&p2=b&p3=5";

			dynamic dynamicQuery = new DynamicQuery(query.ExtractQueryString());

			Assert.AreEqual((string)dynamicQuery.p1, "a");
			Assert.AreEqual((string)dynamicQuery.p2, "b");
			Assert.AreEqual((int)dynamicQuery.p3, 5);
		}
	}
}
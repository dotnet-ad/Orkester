using System;

namespace Orkester.Sample
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			const string query = "a/b/c?p1=5&p2=b&p3=5";

			dynamic dynamicQuery = new DynamicQuery(query.ExtractQueryString());

			int p1 = dynamicQuery.p1;
			int p2 = (int)dynamicQuery.p1;

			Console.ReadKey();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Orkester
{
	public static class QueryExtensions
	{
		public static string ExtractQueryPath(this string query)
		{
			var splits = query?.Split('?');

			if (splits.Length > 1)
			{
				return splits.First();
			}

			return query;
		}

		public static string ExtractQueryString(this string query)
		{
			var path = query.ExtractQueryPath();

			if (query.Length > path.Length)
			{
				return query.Substring(path.Length);
			}

			return null;
		}

		public static string ToOrderedQueryString(this string query)
		{
			return ToOrderedQueryString(ToQueryDictionnary(query));
		}

		public static string ToOrderedQueryString(this Dictionary<string, string> dictionary)
		{
			if (dictionary.Count == 0)
			{
				return string.Empty;
			}

			var args = string.Join("&", dictionary.OrderBy((kvp) => kvp.Key).Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value))));
			return $"?{args}";
		}

		public static Dictionary<string, string> ToQueryDictionnary(this string query)
		{
			return query.TrimStart('?').Split('&').ToDictionary(x => Uri.UnescapeDataString(x.Split('=')[0]), x => Uri.UnescapeDataString(x.Split('=')[1]));
		}
	}
}


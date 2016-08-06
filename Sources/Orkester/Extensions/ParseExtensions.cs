using System;
namespace Orkester
{
	public static class ParseExtensions
	{
		public static T Parse<T>(this string s)
		{
			return (T)Parse(s, typeof(T));
		}

		public static object Parse(this string s, Type t)
		{
			if (t == typeof(string)) return s;
			if (t == typeof(double)) return double.Parse(s);
			if (t == typeof(float)) return float.Parse(s);
			if (t == typeof(int)) return int.Parse(s);
			if (t == typeof(long)) return long.Parse(s);
			if (t == typeof(bool)) return bool.Parse(s);
			if (t == typeof(DateTime)) return DateTime.Parse(s);

			throw new ArgumentException($"Parsing of value as {t} is not supported for : {s}");
		}
	}
}


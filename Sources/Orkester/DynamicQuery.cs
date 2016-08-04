namespace Orkester
{
	using System.Collections.Generic;
	using System.Dynamic;

	/// <summary>
	/// A query string as a dynamic object for accessing query parameters.
	/// </summary>
	public class DynamicQuery : DynamicObject
	{
		public class Parameter
		{
			public Parameter(string value)
			{
				this.value = value;
			}

			private string value;

			public static implicit operator double(Parameter p)
			{
				return double.Parse(p.value);
			}

			public static implicit operator int(Parameter p)
			{
				return int.Parse(p.value);
			}

			public static implicit operator bool(Parameter p)
			{
				return bool.Parse(p.value);
			}

			public static implicit operator string(Parameter p)
			{
				return p.value;
			}
		}

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Orkester.DynamicQuery"/> class.
		/// </summary>
		public DynamicQuery() : this(new Dictionary<string, string>())
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Orkester.DynamicQuery"/> class.
		/// </summary>
		/// <param name="query">A query string.</param>
		public DynamicQuery(string query) : this(query.ToQueryDictionnary())
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Orkester.DynamicQuery"/> class.
		/// </summary>
		/// <param name="query">A query parameter dictionary.</param>
		public DynamicQuery(Dictionary<string, string> query)
		{
			this.dictionary = new Dictionary<string, string>(query);
		}

		#endregion

		#region Fields

		private readonly Dictionary<string, string> dictionary;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the query string.
		/// </summary>
		/// <value>The query string.</value>
		public string QueryString
		{
			get { return dictionary.ToOrderedQueryString(); }
		}

		#endregion

		#region Property accessors

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			return base.TryConvert(binder, out result);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			string val;

			if (dictionary.TryGetValue(binder.Name, out val))
			{
				result = new Parameter(val);
				return true;
			}

			result = null;

			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			dictionary[binder.Name] = value?.ToString();
			return true;
		}

		#endregion
	}
}
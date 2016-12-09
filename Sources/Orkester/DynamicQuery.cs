namespace Orkester
{
	using System;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Linq;

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

			public static implicit operator float(Parameter p)
			{
				return p.To<float>();
			}

			public static implicit operator double(Parameter p)
			{
				return p.To<double>();
			}

			public static implicit operator int(Parameter p)
			{
				return p.To<int>();
			}

			public static implicit operator long(Parameter p)
			{
				return p.To<long>();
			}

			public static implicit operator bool(Parameter p)
			{
				return p.To<bool>();
			}

			public static implicit operator DateTime(Parameter p)
			{
				return p.To<DateTime>();
			}

			public static implicit operator string(Parameter p)
			{
				return p.value;
			}

			public T To<T>()
			{
				return this.value.Parse<T>();
			}

			public object To(Type t)
			{
				return this.value.Parse(t);
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

		public Dictionary<string, Parameter> Values { get { return this.dictionary.ToDictionary((kvp) => kvp.Key, (kvp) => new Parameter(kvp.Value)); } }

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
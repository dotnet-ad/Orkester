namespace Orkester
{
	using System;

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class ScheduledAttribute : Attribute
	{
		public string Path
		{
			get;
			private set;
		}

		public ScheduledAttribute(string path)
		{
			Path = path;
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class ParameterAttribute : Attribute
	{
		public string Name { get; private set; }

		public ParameterAttribute(string name)
		{
			this.Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class RequiredAttribute : ParameterAttribute
	{
		public RequiredAttribute(string name) : base(name)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class OptionnalAttribute : ParameterAttribute
	{
		public OptionnalAttribute(string name) : base(name)
		{
		}
	}

	#region Behaviors

	[AttributeUsage(AttributeTargets.Method)]
	public abstract class BehaviorAttribute : Attribute
	{
		public int Priority { get; private set; }

		public BehaviorAttribute (int priority)
		{
			this.Priority = priority;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithRepeatAttribute : BehaviorAttribute
	{
		public int Times { get; private set; }

		public WithRepeatAttribute(int times, int priority = 3) : base(priority)
		{
			Times = times;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithConcurrentAttribute : BehaviorAttribute
	{
		public int Limit { get; private set; }

		public WithConcurrentAttribute(int limit, int priority = 0) : base(priority)
		{
			Limit = limit;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithLockAttribute : BehaviorAttribute
	{
		public WithLockAttribute(int priority = 0) : base(priority)
		{
			
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithUniquenessAttribute : BehaviorAttribute
	{
		public WithUniquenessAttribute(int priority = 0) : base(priority)
		{

		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithCurrentAttribute : BehaviorAttribute
	{
		public WithCurrentAttribute(int priority = 0) : base(priority)
		{

		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithAggregationAttribute : BehaviorAttribute
	{
		public TimeSpan Span { get; private set; }

		public WithAggregationAttribute(TimeSpan span,int priority = 0) : base(priority)
		{
			Span = span;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithExpirationAttribute : BehaviorAttribute
	{
		public TimeSpan Span { get; private set; }

		public WithExpirationAttribute(TimeSpan span,int priority = 0) : base(priority)
		{
			Span = span;
		}
	}


	[AttributeUsage(AttributeTargets.Method)]
	public class WithTimeoutAttribute : BehaviorAttribute
	{
		public TimeSpan Span { get; private set; }

		public WithTimeoutAttribute(TimeSpan span, int priority = 0) : base(priority)
		{
			Span = span;
		}
	}


	#endregion
}


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

	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class ParameterAttribute : Attribute
	{
		public string Name { get; private set; }

		public ParameterAttribute(string name)
		{
			this.Name = name;
		}
	}

	#region Behaviors

	[AttributeUsage(AttributeTargets.Method)]
	public abstract class BehaviorAttribute : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithRepeatAttribute : BehaviorAttribute
	{
		public int Times { get; private set; }

		public WithRepeatAttribute(int times)
		{
			Times = times;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WithUniquenessAttribute : BehaviorAttribute
	{
	}

	#endregion
}


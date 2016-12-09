namespace Orkester
{
	using System;
	using System.Reflection;
	using System.Threading.Tasks;

	public class Execution : IExecution
	{
		public Execution(IOperation operation, DynamicQuery query, Task task)
		{
			this.Operation = operation;
			this.Query = query;
			this.Task = task;
			this.StartDate = DateTime.Now;
			this.Task.ContinueWith((t) =>
			{
				this.EndDate = DateTime.Now;
			});
		}

		public IOperation Operation
		{
			get; private set;
		}

		public DynamicQuery Query
		{
			get; private set;
		}

		public DateTime? EndDate
		{
			get; private set;
		}

		public DateTime StartDate
		{
			get; private set;
		}

		public Task Task
		{
			get; private set;
		}

		public bool IsFinished
		{
			get { return this.EndDate != null; }
		}
	}
}


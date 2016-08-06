namespace Orkester
{
	using System;
	using System.Reflection;
	using System.Threading.Tasks;

	public class Execution : IExecution
	{
		public Execution(string query, Task task)
		{
			this.Query = query;
			this.Task = task;
			this.StartDate = DateTime.Now;
			this.Task.ContinueWith((t) =>
			{
				this.EndDate = DateTime.Now;
			});
		}

		public string Query
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

		public Task<T> AsTask<T>()
		{
			return this.Task as Task<T>;
		}
	}
}


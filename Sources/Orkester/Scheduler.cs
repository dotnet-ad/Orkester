using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orkester
{
	public class Scheduler : IScheduler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Orkester.Orkester"/> class.
		/// </summary>
		public Scheduler()
		{
			
		}

		#region Global default instance

		private static readonly Lazy<IScheduler> instance = new Lazy<IScheduler>(() => new Scheduler());

		/// <summary>
		/// Gets a default global instance of <see cref="T:Orkester.IOrkester"/>.
		/// </summary>
		/// <value>The default instance.</value>
		public static IScheduler Default
		{
			get { return instance.Value; }
		}

		#endregion

		#region Creation

		public IOperation<Void> Create(Func<dynamic, CancellationToken, Task> task)
		{
			return new Operation<Void>(this, async (dq, ct) =>
			{
				await task(dq, ct);
				return Void.Value;
			});
		}

		public IOperation<T> Create<T>(Func<dynamic, CancellationToken, Task<T>> task)
		{
			return new Operation<T>(this, task);
		}

		#endregion

		#region Registration

		private Dictionary<string, object> operations = new Dictionary<string, object>();

		public IOperation<T> Register<T>(string name, IOperation<T> operation)
		{
			if (this.operations.ContainsKey(name))
			{
				throw new InvalidOperationException($"An operation has already been registered with the name \"{name}\".");
			}

			this.operations[name] = operation;

			return operation;
		}

		/// <summary>
		/// Finds the operation from a given query.
		/// </summary>
		/// <returns>The operation.</returns>
		/// <param name="query">Query.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		private IOperation<T> FindOperation<T>(string query)
		{
			var path = query.ExtractQueryPath();

			if (!this.operations.ContainsKey(path))
			{
				throw new InvalidOperationException($"There's no operation registered with the name \"{path}\".");
			}

			return this.operations[path] as IOperation<T>;
		}

		#endregion

		#region Execution

		public Task<T> ExecuteAsync<T>(string query, CancellationToken token = default(CancellationToken))
		{
			var operation = this.FindOperation<T>(query);
			var dynamicQuery = new DynamicQuery(query.ExtractQueryString());
			return operation.ExecuteAsync(dynamicQuery, token);
		}

		public Task ExecuteAsync(string query, CancellationToken token = default(CancellationToken))
		{
			return this.ExecuteAsync<Void>(query, token);
		}

		#endregion
	}
}
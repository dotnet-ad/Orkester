namespace Orkester
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Centralized manager for asynchronous operations.
	/// </summary>
	public interface IScheduler
	{
		#region Creation

		/// <summary>
		/// Initializes an operation from an asynchronous task factory.
		/// </summary>
		/// <param name="task">The task factory from query parameters.</param>
		IOperation<Void> Create(Func<dynamic, CancellationToken, Task> task);

		/// <summary>
		/// Initializes an operation that has a returns value from an asynchronous task factory.
		/// </summary>
		/// <param name="task">The task factory from query parameters.</param>
		IOperation<T> Create<T>(Func<dynamic, CancellationToken, Task<T>> task);

		#endregion

		#region Registration

		/// <summary>
		/// Register the specified operation for the given name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="operation">Operation.</param>
		IOperation<T> Register<T>(string name, IOperation<T> operation);

		#endregion

		#region Execution

		/// <summary>
		/// Executes the operation corresponding to the specified query.
		/// </summary>
		/// <param name="query">Query.</param>
		/// <param name="token">Token.</param>
		Task ExecuteAsync(string query, CancellationToken token = default(CancellationToken));

		/// <summary>
		/// Executes the operation corresponding to the specified query.
		/// </summary>
		/// <param name="query">Query.</param>
		/// <param name="token">Token.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		Task<T> ExecuteAsync<T>(string query, CancellationToken token = default(CancellationToken));

		#endregion
	}
}


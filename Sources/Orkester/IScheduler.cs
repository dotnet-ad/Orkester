namespace Orkester
{
	using System;
	using System.Collections.ObjectModel;
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
		IOperation Create(Func<dynamic, CancellationToken, Task> task);

		/// <summary>
		/// Initializes an operation that has a returns value from an asynchronous task factory.
		/// </summary>
		/// <param name="task">The task factory from query parameters.</param>
		IOperation Create<T>(Func<dynamic, CancellationToken, Task<T>> task);

		#endregion

		#region Registration

		/// <summary>
		/// Register the specified operation for the given name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="operation">Operation.</param>
		IOperation Register<T>(string name, IOperation operation);

		#endregion

		#region Chains and groups

		/// <summary>
		/// Creates an operation that executes a set of other registered operations sequencially.
		/// </summary>
		/// <param name="operations">Operations.</param>
		IOperation Chain(params string[] operations);

		/// <summary>
		/// Creates an operation that executes a set of other registered operations concurently.
		/// </summary>
		/// <param name="operations">Operations.</param>
		IOperation Group(params string[] operations);

		#endregion

		#region Execution

		/// <summary>
		/// Gets the currently executed queries.
		/// </summary>
		/// <value>The current queries.</value>
		ObservableCollection<IExecution> CurrentQueries { get; }

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

		#region Planification

		//void Planify(string query, TimeSpan span, bool loop);

		//void Unplanify(string query);

		#endregion

		#region Service

		/// <summary>
		/// Registers the specified service. This service must be decorated with specific attributes (ScheduledAttributes,
		/// BehaviorAttributes, ...).
		/// </summary>
		/// <param name="service">Service.</param>
		/// <typeparam name="TService">The 1st type parameter.</typeparam>
		void Register<TService>(Func<TService> service);

		#endregion
	}
}


namespace Orkester
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// An operation
	/// </summary>
	public interface IOperation
	{
		/// <summary>
		/// Asynchronous task executed when operation is executed.
		/// </summary>
		/// <value>The execute async.</value>
		Func<dynamic, CancellationToken, Task> ExecuteAsync { get; }

		/// <summary>
		/// Only a max number of operation's tasks will be executed at a given time.
		/// </summary>
		/// <returns>The max concurrent.</returns>
		/// <param name="limit">Limit.</param>
		IOperation WithMaxConcurrent(int limit);

		/// <summary>
		/// Only one operation's task will be executed at a given time.
		/// </summary>
		/// <returns>The lock.</returns>
		IOperation WithLock();

		/// <summary>
		/// The operation's task will be repeated the given number of times.
		/// </summary>
		/// <returns>The repeat.</returns>
		/// <param name="times">Times.</param>
		IOperation WithRepeat(int times);

		/// <summary>
		/// If the operation's task takes more than the specified timespan, it is cancelled.
		/// </summary>
		/// <returns>The timeout.</returns>
		/// <param name="span">Span.</param>
		IOperation WithTimeout(TimeSpan span);

		/// <summary>
		/// Only one succesful operation's task will be executed and the same result will always be returned.
		/// </summary>
		/// <returns>The current.</returns>
		IOperation WithUniqueness();

		/// <summary>
		/// If a same operation's task is currently running, that task will be awaited instead of a new one.
		/// </summary>
		/// <returns>The current.</returns>
		IOperation WithCurrent();

		/// <summary>
		/// The operation's task will wait a period of time before starting. During this period, all execution
		/// requests will be ignored and only the initial request will be awaited by all requesters.
		/// </summary>
		/// <returns>The aggregation.</returns>
		/// <param name="span">Span.</param>
		IOperation WithAggregation(TimeSpan span);

		/// <summary>
		/// The result of a previous operation's task will be returned if already executed until it expires.
		/// </summary>
		/// <returns>The expiration.</returns>
		/// <param name="span">Span.</param>
		IOperation WithExpiration(TimeSpan span);

		/// <summary>
		/// Registers the task for later executions.
		/// </summary>
		/// <param name="name">Name.</param>
		IOperation Save(string name);
	}
}
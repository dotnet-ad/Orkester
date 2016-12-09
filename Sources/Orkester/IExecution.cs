namespace Orkester
{
	using System;
	using System.Threading.Tasks;

	/// <summary>
	/// Represents an operation execution state.
	/// </summary>
	public interface IExecution
	{
		/// <summary>
		/// Gets the start date.
		/// </summary>
		/// <value>The start date.</value>
		DateTime StartDate { get; }

		/// <summary>
		/// Gets the end date (or null if execution isn't finished yet).
		/// </summary>
		/// <value>The end date.</value>
		DateTime? EndDate { get; }

		/// <summary>
		/// Gets or sets the executed query.
		/// </summary>
		/// <value>The query.</value>
		string Query { get; }

		/// <summary>
		/// Gets the asynchronous task associated to this execution.
		/// </summary>
		/// <value>The task.</value>
		Task Task { get; }

		/// <summary>
		/// Gets the result if Task has one.
		/// </summary>
		/// <returns>The result.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		Task<T> AsTask<T>();
	}
}


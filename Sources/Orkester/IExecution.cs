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
		/// Indicates whether the execution is finished.
		/// </summary>
		/// <value>The is finished.</value>
		bool IsFinished { get; }

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
		/// Gets the operation representation.
		/// </summary>
		/// <value>The operation.</value>
		IOperation Operation { get; }

		/// <summary>
		/// Gets the query parameters.
		/// </summary>
		/// <value>The query.</value>
		DynamicQuery Query { get; }

		/// <summary>
		/// Gets the asynchronous task associated to this execution.
		/// </summary>
		/// <value>The task.</value>
		Task Task { get; }
	}
}


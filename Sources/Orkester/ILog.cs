namespace Orkester
{
	using System;

	/// <summary>
	/// An operation logger.
	/// </summary>
	public interface ILog
	{
		bool IsEnabled { get; set; }

		event EventHandler<IExecution> ExecutionRequested;

		event EventHandler<IExecution> ExecutionFailed;

	 	event EventHandler<IExecution> ExecutionEnded;

		event EventHandler<IExecution> ExecutionStarted;
	}
}
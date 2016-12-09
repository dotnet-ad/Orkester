using System;
namespace Orkester
{
	public class Log : ILog
	{
		public Log()
		{
		}

		public bool IsEnabled
		{
			get; set;
		}

		public void RaiseExecutionEnded(IExecution e)
		{
			if (this.IsEnabled)
			{
				ExecutionEnded?.Invoke(this, e);
			}
		}

		public void RaiseExecutionFailed(IExecution e)
		{
			if (this.IsEnabled)
			{
				ExecutionFailed?.Invoke(this, e);
			}
		}

		public void RaiseExecutionRequested(IExecution e)
		{
			if (this.IsEnabled)
			{
				ExecutionRequested?.Invoke(this, e);
			}
		}

		public void RaiseExecutionStarted(IExecution e)
		{
			if (this.IsEnabled)
			{
				ExecutionStarted?.Invoke(this, e);
			}
		}

		public event EventHandler<IExecution> ExecutionEnded;

		public event EventHandler<IExecution> ExecutionFailed;

		public event EventHandler<IExecution> ExecutionRequested;

		public event EventHandler<IExecution> ExecutionStarted;

	}
}


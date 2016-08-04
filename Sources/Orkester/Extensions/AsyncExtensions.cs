namespace Orkester
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;

	public static class AsyncExtensions
	{
		public static Func<CancellationToken, Task<T>> WithLock<T>(this Func<CancellationToken, Task<T>> func)
		{
			return func.WithMaxConcurrent(1);
		}

		public static Func<CancellationToken, Task<T>> WithMaxConcurrent<T>(this Func<CancellationToken, Task<T>> func, int limit)
		{
			var semaphore = new SemaphoreSlim(limit);

			return async (ct) =>
			{
				await semaphore.WaitAsync();
				var r = await func(ct);
				semaphore.Release();
				return r;
			};
		}

		public static Func<CancellationToken, Task<IEnumerable<T>>> WithRepeat<T>(this Func<CancellationToken, Task<T>> func, int times)
		{
			return async (ct) =>
			{
				var result = new List<T>();
				for (int i = 0; i < times; i++)
				{
					result.Add(await func(ct));
				}
				return (IEnumerable<T>)result;
			};
		}

		public static Func<CancellationToken, Task<T>> WithTimeout<T>(this Func<CancellationToken, Task<T>> func, TimeSpan span)
		{
			return async (ct) =>
			{
				var task = func(ct);
				var delay = Task.Delay(span, ct);

				if (await Task.WhenAny(task, delay) == delay)
				{
					throw new OperationTimedOutException();
				}

				return await task;
			};
		}

		/// <summary>
		/// The task is only executed one time and then returned for next requests.
		/// </summary>
		/// <returns>The uniqueness.</returns>
		/// <param name="func">Func.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static Func<CancellationToken,Task<T>> WithUniqueness<T>(this Func<CancellationToken,Task<T>> func)
		{
			Task<T> task = null;

			return (ct) =>
			{
				if (task == null)
				{
					task = func(ct);
				}

				return task;
			};
		}

		public static Func<CancellationToken, Task<T>> WithAggregation<T>(this Func<CancellationToken, Task<T>> func, TimeSpan span)
		{
			Task<T> task = null;
			Task wait = null;

			return async (ct) =>
			{
				if (wait == null)
				{
					wait = Task.Delay(span, ct).ContinueWith((t) =>
					{
						task = func(ct);
						wait = null;
					});
				}

				await wait;

				return await task;
			};
		}

		/// <summary>
		/// If a task has finished before its expiration given span it is returned.
		/// </summary>
		/// <returns>The expiration.</returns>
		/// <param name="func">Func.</param>
		/// <param name="span">Span.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static Func<CancellationToken, Task<T>> WithExpiration<T>(this Func<CancellationToken, Task<T>> func, TimeSpan span)
		{
			Task<T> task = null;
			DateTime date = DateTime.MinValue;

			return async (ct) =>
			{
				if (task != null && ((!task.IsCompleted) || (task.Status == TaskStatus.RanToCompletion && ((date + span) > DateTime.Now))))
				{
					return await task;
				}

				if (task != null && task.Status == TaskStatus.RanToCompletion)
				{
					int i = 9;
				}

				task = func(ct);
				var r = await task;
				date = DateTime.Now;
				return r;
			};
		}

		/// <summary>
		/// If a task is already currently executed, then it is returned instead of a new one.
		/// </summary>
		/// <returns>The current.</returns>
		/// <param name="func">Func.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static Func<CancellationToken, Task<T>> WithCurrent<T>(this Func<CancellationToken, Task<T>> func)
		{
			return func.WithExpiration(TimeSpan.FromMilliseconds(0));
		}
	}
}
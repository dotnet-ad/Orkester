namespace Orkester
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	public static class AsyncExtensions
	{

		#region With one argument

		public static Func<TArg, CancellationToken, Task<T>> WithLock<TArg, T>(this Func<TArg, CancellationToken, Task<T>> func)
		{
			return func.WithMaxConcurrent(1);
		}

		public static Func<TArg, CancellationToken, Task<T>> WithMaxConcurrent<TArg,T>(this Func<TArg, CancellationToken, Task<T>> func, int limit)
		{
			var semaphore = new SemaphoreSlim(limit);

			return async (a,ct) =>
			{
				await semaphore.WaitAsync();
				var r = await func(a,ct);
				semaphore.Release();
				return r;
			};
		}

		public static Func<TArg, CancellationToken, Task<IEnumerable<T>>> WithRepeat<TArg, T>(this Func<TArg, CancellationToken, Task<T>> func, int times)
		{
			return async (a,ct) =>
			{
				var result = new List<T>();
				for (int i = 0; i < times; i++)
				{
					result.Add(await func(a,ct));
				}
				return (IEnumerable<T>)result;
			};
		}

		public static Func<TArg, CancellationToken, Task<T>> WithTimeout<TArg,T>(this Func<TArg, CancellationToken, Task<T>> func, TimeSpan span)
		{
			return async (a,ct) =>
			{
				var task = func(a,ct);
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
		public static Func<TArg, CancellationToken,Task<T>> WithUniqueness<TArg, T>(this Func<TArg, CancellationToken,Task<T>> func)
		{
			Task<T> task = null;

			return (a,ct) =>
			{
				if (task == null)
				{
					task = func(a,ct);
				}

				return task;
			};
		}

		public static Func<TArg, CancellationToken, Task<T>> WithAggregation<TArg, T>(this Func<TArg, CancellationToken, Task<T>> func, TimeSpan span)
		{
			Task<T> task = null;
			Task wait = null;

			return async (a,ct) =>
			{
				if (wait == null)
				{
					wait = Task.Delay(span, ct).ContinueWith((t) =>
					{
						task = func(a,ct);
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
		public static Func<TArg, CancellationToken, Task<T>> WithExpiration<TArg, T>(this Func<TArg, CancellationToken, Task<T>> func, TimeSpan span)
		{
			Task<T> task = null;
			DateTime date = DateTime.MinValue;

			return async (a,ct) =>
			{
				if (task != null && ((!task.IsCompleted) || (task.Status == TaskStatus.RanToCompletion && ((date + span) > DateTime.Now))))
				{
					return await task;
				}

				task = func(a,ct);
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
		public static Func<TArg, CancellationToken, Task<T>> WithCurrent<TArg, T>(this Func<TArg, CancellationToken, Task<T>> func)
		{
			return func.WithExpiration(TimeSpan.FromMilliseconds(0));
		}

		#endregion

		#region Without args

		public static Func<CancellationToken, Task<T>> WithLock<T>(this Func<CancellationToken, Task<T>> func)
		{
			return func.WithMaxConcurrent(1);
		}

		public static Func<CancellationToken, Task<T>> WithMaxConcurrent<T>(this Func<CancellationToken, Task<T>> func, int limit)
		{
			var op = WithMaxConcurrent<Void, T>((a, ct) => func(ct), limit);
			return (ct) => op(Void.Value, ct);
		}

		public static Func<CancellationToken, Task<IEnumerable<T>>> WithRepeat<T>(this Func<CancellationToken, Task<T>> func, int times)
		{
			var op = WithRepeat<Void, T>((a, ct) => func(ct), times);
			return (ct) => op(Void.Value, ct);
		}

		public static Func<CancellationToken, Task<T>> WithTimeout<T>(this Func<CancellationToken, Task<T>> func, TimeSpan span)
		{
			var op = WithTimeout<Void, T>((a, ct) => func(ct), span);
			return (ct) => op(Void.Value, ct);
		}

		public static Func<CancellationToken, Task<T>> WithUniqueness<T>(this Func<CancellationToken, Task<T>> func)
		{
			var op = WithUniqueness<Void, T>((a, ct) => func(ct));
			return (ct) => op(Void.Value, ct);
		}

		public static Func<CancellationToken, Task<T>> WithAggregation<T>(this Func<CancellationToken, Task<T>> func, TimeSpan span)
		{
			var op = WithAggregation<Void, T>((a, ct) => func(ct), span);
			return (ct) => op(Void.Value, ct);
		}

		public static Func<CancellationToken, Task<T>> WithExpiration<T>(this Func<CancellationToken, Task<T>> func, TimeSpan span)
		{
			var op = WithExpiration<Void, T>((a, ct) => func(ct), span);
			return (ct) => op(Void.Value, ct);
		}

		public static Func<CancellationToken, Task<T>> WithCurrent<T>(this Func<CancellationToken, Task<T>> func)
		{
			var op = WithCurrent<Void, T>((a, ct) => func(ct));
			return (ct) => op(Void.Value, ct);
		}

		#endregion
	}
}
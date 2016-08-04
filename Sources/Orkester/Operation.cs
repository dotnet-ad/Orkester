namespace Orkester
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Base implementation of operations.
	/// </summary>
	public class Operation<T> : IOperation<T>
	{
		public Operation(IScheduler parent, Func<dynamic, CancellationToken,Task<T>> createTask)
		{
			this.parent = parent;
			this.ExecuteAsync = createTask;
		}

		readonly IScheduler parent;

		public Func<dynamic, CancellationToken, Task<T>> ExecuteAsync { get; private set; }

		private Operation<TResult> Clone<TResult>(Func<dynamic, CancellationToken, Task<TResult>> executeAsync)
		{
			return new Operation<TResult>(this.parent,executeAsync);
		}

		public IOperation<IEnumerable<T>> WithRepeat(int times)
		{
			return this.Clone<IEnumerable<T>>(async (dq, ct) =>
			{
				var f = ((Func<CancellationToken, Task<T>>)((ctt) => ExecuteAsync(dq, ctt))).WithRepeat(times);
				return await f(ct);
			});
		}

		public IOperation<T> WithTimeout(TimeSpan span)
		{
			return this.Clone<T>(async (dq, ct) =>
			{
				var f = ((Func<CancellationToken, Task<T>>)((ctt) => ExecuteAsync(dq, ctt))).WithTimeout(span);
				return await f(ct);
			});
		}

		private IOperation<T> PerQuery(Func<Func<CancellationToken, Task<T>>, Func<CancellationToken, Task<T>>> convert)
		{
			var factories = new Dictionary<string, Func<CancellationToken, Task<T>>>();

			return this.Clone((dq, ct) =>
			{
				var key = ((DynamicQuery)dq).QueryString;
				Func<CancellationToken, Task<T>> func;
				if (!factories.TryGetValue(key, out func))
				{
					func = convert(((Func<CancellationToken, Task<T>>)((ctt) => ExecuteAsync(dq, ctt))));
					factories[key] = func;
				}

				return func(ct);
			});
		}

		public IOperation<T> WithLock()
		{
			return PerQuery((f) => f.WithLock());
		}

		public IOperation<T> WithMaxConcurrent(int limit)
		{
			return PerQuery((f) => f.WithMaxConcurrent(limit));
		}

		public IOperation<T> WithUniqueness()
		{
			return PerQuery((f) => f.WithUniqueness());
		}

		public IOperation<T> WithCurrent()
		{
			return PerQuery((f) => f.WithCurrent());
		}

		public IOperation<T> WithAggregation(TimeSpan span)
		{
			return PerQuery((f) => f.WithAggregation(span));
		}

		public IOperation<T> WithExpiration(TimeSpan span)
		{
			return PerQuery((f) => f.WithExpiration(span));
		}

		public IOperation<T> Save(string name)
		{
			return this.parent.Register<T>(name, this);
		}
	}
}
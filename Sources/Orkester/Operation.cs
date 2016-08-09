namespace Orkester
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Base implementation of operations.
	/// </summary>
	public class Operation<T> : IOperation
	{
		public Operation(IScheduler parent, Func<dynamic, CancellationToken,Task<T>> createTask)
		{
			this.parent = parent;
			this.executeAsync = createTask;
		}

		public Operation(IScheduler parent, Func<dynamic, CancellationToken, Task> createTask)
		{
			this.parent = parent;
			this.executeAsync = (dq, ct) =>
			{
				return createTask(dq,ct) as Task<T>;
			};
		}

		readonly IScheduler parent;

		private Func<dynamic, CancellationToken, Task<T>> executeAsync;

		public Func<dynamic, CancellationToken, Task<T>> GenericExecuteAsync { get { return executeAsync; } }

		public Func<dynamic, CancellationToken, Task> ExecuteAsync { get { return executeAsync; } }

		private Operation<TReturn> Clone<TReturn>(Func<dynamic, CancellationToken, Task<TReturn>> newExecuteAsync)
		{
			return new Operation<TReturn>(this.parent,newExecuteAsync);
		}

		public IOperation WithRepeat(int times)
		{
			var f = GenericExecuteAsync.WithRepeat(times);

			return this.Clone(async (dq, ct) =>
			{
				return await f(dq,ct);
			});
		}

		public IOperation WithTimeout(TimeSpan span)
		{
			var f = GenericExecuteAsync.WithTimeout(span);

			return this.Clone(async (dq, ct) =>
			{
				return await f(dq,ct);
			});
		}

		private IOperation PerQuery(Func<Func<dynamic, CancellationToken, Task<T>>, Func<dynamic,CancellationToken, Task<T>>> convert)
		{
			var factories = new Dictionary<string, Func<dynamic,CancellationToken, Task<T>>>();

			return this.Clone<T>((dq, ct) =>
			{
				var key = ((DynamicQuery)dq).QueryString;
				Func<dynamic, CancellationToken, Task<T>> func;

				if (!factories.TryGetValue(key, out func))
				{
					func = convert(GenericExecuteAsync);
					factories[key] = func;
				}

				return func(dq, ct);
			});

		}

		public IOperation WithLock()
		{
			return PerQuery((f) => f.WithLock());
		}

		public IOperation WithMaxConcurrency(int limit)
		{
			return PerQuery((f) => f.WithMaxConcurrency(limit));
		}

		public IOperation WithUniqueness()
		{
			return PerQuery((f) => f.WithUniqueness());
		}

		public IOperation WithCurrent()
		{
			return PerQuery((f) => f.WithCurrent());
		}

		public IOperation WithAggregation(TimeSpan span)
		{
			return PerQuery((f) => f.WithAggregation(span));
		}

		public IOperation WithExpiration(TimeSpan span)
		{
			return PerQuery((f) => f.WithExpiration(span));
		}

		public IOperation Save(string name)
		{
			return this.parent.Register<T>(name, this);
		}
	}

	public class Operation : Operation<Void>
	{
		public Operation(IScheduler parent, Func<dynamic, CancellationToken, Task> createTask) : base(parent, (async (q, ct) => { await createTask(q, ct); return Void.Value; }))
		{
		}
	}
}
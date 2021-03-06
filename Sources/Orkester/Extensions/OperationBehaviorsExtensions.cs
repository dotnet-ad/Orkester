﻿using System;
namespace Orkester
{
	public static class OperationBehaviorsExtensions
	{
		public static IOperation WithRepeat(this IOperation operation, int times)
		{
			var f = operation.GenericExecuteAsync.WithRepeat(times);

			return this.Clone(async (dq, ct) =>
			{
				return await f(dq, ct);
			});
		}

		public IOperation WithTimeout(TimeSpan span)
		{
			var f = GenericExecuteAsync.WithTimeout(span);

			return this.Clone(async (dq, ct) =>
			{
				return await f(dq, ct);
			});
		}

		private IOperation PerQuery(Func<Func<dynamic, CancellationToken, Task<T>>, Func<dynamic, CancellationToken, Task<T>>> convert)
		{
			var factories = new Dictionary<string, Func<dynamic, CancellationToken, Task<T>>>();

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
	}
}


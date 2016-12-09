using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Orkester.Tests
{
	[TestFixture()]
	public class AsyncExtensionsTests
	{
		#region Helper

		private Func<CancellationToken, Task<bool>> Create(Func<CancellationToken, Task> func)
		{
			return (async (ct) =>
			{
				await func(ct);
				return true;
			});
		}

		private Func<CancellationToken, Task<T>> Create<T>(Func<CancellationToken, Task<T>> func)
		{
			return func;
		}

		private CancellationToken ct = default(CancellationToken);

		#endregion

		#region Concurrency

		[Test()]
		public async Task ShouldUserMaxConcurent()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				count++;
				await Task.Delay(100);
				Assert.IsTrue(count <= 3);
				count--;
			}).WithMaxConcurrent(3);

			await Task.WhenAll(
				f(ct),
				f(ct),
				f(ct),
				f(ct),
				f(ct)
			);
		}

		[Test()]
		public async Task ShouldLock()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				count++;
				await Task.Delay(100);
				Assert.AreEqual(1, count);
				count--;
			}).WithLock();

			await Task.WhenAll(
				f(ct),
				f(ct),
				f(ct),
				f(ct),
				f(ct)
			);
		}

		#endregion

		#region Repeat

		[Test()]
		public async Task ShouldRepeat()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				await Task.Delay(100);
				count++;
			}).WithRepeat(3);

			await f(ct);

			Assert.AreEqual(3, count);
		}

		#endregion

		#region Timeout

		[Test()]
		public async Task ShouldTimeout()
		{
			var f = Create(async (ct) =>
			{
				await Task.Delay(500);
			}).WithTimeout(TimeSpan.FromMilliseconds(100));

			try
			{
				await f(ct);
				Assert.Fail();
			}
			catch (OperationTimedOutException) {}
		}

		#endregion

		#region Uniqueness

		[Test()]
		public async Task ShouldBeUnique()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				await Task.Delay(100);
				count++;
			}).WithUniqueness();

			await Task.WhenAll(f(ct),f(ct));
			await f(ct);

			Assert.AreEqual(1, count);
		}
		#endregion

		#region Current

		[Test()]
		public async Task ShouldUseCurrent()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				await Task.Delay(200);
				count++;
			}).WithCurrent();

			var t1 = Task.WhenAll(f(ct), f(ct));
			await Task.Delay(100);
			var t2 = f(ct);

			await Task.WhenAll(t1, t2);

			Assert.AreEqual(1, count);

			await f(ct);

			Assert.AreEqual(2, count);
		}

		#endregion

		#region Aggregation

		[Test()]
		public async Task ShouldAggregate()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				count++;
				await Task.Delay(50);
				count--;
			}).WithAggregation(TimeSpan.FromMilliseconds(200));

			var t1 = f(ct);
			await Task.Delay(50);

			Assert.AreEqual(0, count);

			var t2 = f(ct);
			await Task.Delay(120);

			Assert.AreEqual(0, count);

			await Task.Delay(40);

			Assert.AreEqual(1, count);

			await Task.WhenAll(t1, t2);

			Assert.AreEqual(0, count);
		}

		#endregion

		#region Expiration

		[Test()]
		public async Task ShouldExpire()
		{
			int count = 0;

			var f = Create(async (ct) =>
			{
				await Task.Delay(50);
				count++;
			}).WithExpiration(TimeSpan.FromMilliseconds(200));

			await f(ct);
			await f(ct);
			await f(ct);

			Assert.AreEqual(1, count);

			await Task.Delay(200);
			await f(ct);

			Assert.AreEqual(2, count);
		}

		#endregion

		#region Multiple

		[Test()]
		public async Task ShouldBeMultipleUnique()
		{
			int count = 0;

			var ta = Create(async (ct) =>
			{
				await Task.Delay(50);
				count++;
			}).WithUniqueness();


			var tb = Create(async (ct) =>
			{
				await Task.Delay(50);
				count++;
			}).WithUniqueness();

			await ta(ct);
			await ta(ct);

			Assert.AreEqual(1, count);

			await tb(ct);
			await tb(ct);

			Assert.AreEqual(2, count);
		}

		#endregion
	}
}


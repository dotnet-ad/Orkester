namespace Orkester.Tests
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using NUnit.Framework;
	using Orkester;

	[TestFixture()]
	public class SchedulerTests
	{
		[Test()]
		public async Task ShouldExecuteFromQuery()
		{
			var scheduler = new Scheduler();

			scheduler.Create(async (query, ct) => { 
				await Task.Delay(100); 
				return (string)query.p1; 
			}).Save("/example");

			var a = await scheduler.ExecuteAsync<string>("/example?p1=A");
			var b = await scheduler.ExecuteAsync<string>("/example?p1=B");

			Assert.AreEqual("A", a);
			Assert.AreEqual("B", b);
		}

		[Test()]
		public async Task ShouldExecuteRepeatFromQuery()
		{
			var scheduler = new Scheduler();

			int count = 0;

			scheduler.Create(async (query, ct) =>
			{
				await Task.Delay(10);
				count += (int)query.i;
			}).WithUniqueness().Save("/add");

			await scheduler.ExecuteAsync("/add?i=2");
			await scheduler.ExecuteAsync("/add?i=3");
			await scheduler.ExecuteAsync("/add?i=3");

			Assert.AreEqual(5, count);
		}

		[Test()]
		public async Task MaxOperationConcurency()
		{
			var scheduler = new Scheduler(2);

			int count = 0;

			scheduler.Create(async (query, ct) =>
			{
				await Task.Delay(400);
				count += (int)query.i;
			}).Save("/add");

			var t = new[]{
				scheduler.ExecuteAsync("/add?i=1"),
				scheduler.ExecuteAsync("/add?i=2"),
				scheduler.ExecuteAsync("/add?i=3")
			};

			await Task.Delay(450);

			Assert.AreEqual(3, count);

			await Task.WhenAll(t);

			Assert.AreEqual(6, count);
		}

		#region Service registration

		[Scheduled("/example")]
		public class ExampleService
		{
			public int Count { get; set; }
			
			[Scheduled("/add")]
			[WithUniqueness]
			public async Task<int> Add(int a, int b)
			{
				await Task.Delay(100);
				Count += a + b;
				return Count;
			}
		}

		[Test()]
		public async Task ShouldRegisterService()
		{
			var service = new ExampleService();

			var scheduler = new Scheduler();
			scheduler.Register(() =>  service);

			var count = await scheduler.ExecuteAsync<int>("/example/add?a=1&b=2");

			Assert.AreEqual(3, count);
		}


		[Test()]
		public async Task ShouldRegisterServiceWithBehaviors()
		{
			var service = new ExampleService();

			var scheduler = new Scheduler();
			scheduler.Register(() => service);

			await scheduler.ExecuteAsync<int>("/example/add?a=1&b=2");
			await scheduler.ExecuteAsync<int>("/example/add?b=2&a=1");

			Assert.AreEqual(3, service.Count);
		}

		#endregion
	}
}
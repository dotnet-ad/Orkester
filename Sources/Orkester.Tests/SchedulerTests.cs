namespace Orkester.Tests
{
	using System;
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
	}
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Orkester
{
	public class Scheduler : IScheduler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Orkester.Orkester"/> class.
		/// </summary>
		public Scheduler(int maxConcurent = 200)
		{
			if (maxConcurent > 0)
			{
				this.semaphore = new SemaphoreSlim(maxConcurent);
			}
		
			this.CurrentQueries = new ObservableCollection<IExecution>();
		}


		#region Global default instance

		private static readonly Lazy<IScheduler> instance = new Lazy<IScheduler>(() => new Scheduler());

		/// <summary>
		/// Gets a default global instance of <see cref="T:Orkester.IOrkester"/>.
		/// </summary>
		/// <value>The default instance.</value>
		public static IScheduler Default
		{
			get { return instance.Value; }
		}

		#endregion

		#region Fields

		private SemaphoreSlim semaphore;

		#endregion

		#region Properties

		public ObservableCollection<IExecution> CurrentQueries { get; private set; }

		#endregion

		#region Creation

		public IOperation Create(Func<dynamic, CancellationToken, Task> task)
		{
			return new Operation(this, task);
		}

		public IOperation Create<T>(Func<dynamic, CancellationToken, Task<T>> task)
		{
			return new Operation<T>(this, task);
		}

		#endregion

		#region Chains and groups

		public IOperation Chain(params string[] operations)
		{
			return this.Create(async (query, ct) =>
			{
				var dq = query as DynamicQuery;

				foreach (var op in operations)
				{
					await this.ExecuteAsync($"{op}{dq.QueryString}", ct);
				}
			});
		}

		public IOperation Group(params string[] operations)
		{
			return this.Create(async (query, ct) =>
			{
				var dq = query as DynamicQuery;

				var tasks = operations.Select((op) => this.ExecuteAsync($"{op}{dq.QueryString}", ct));

				await Task.WhenAll(tasks);
			});
		}

		#endregion

		#region Registration

		private Dictionary<string, IOperation> operations = new Dictionary<string, IOperation>();

		public IOperation Register<T>(string name, IOperation operation)
		{
			if (this.operations.ContainsKey(name))
			{
				throw new InvalidOperationException($"An operation has already been registered with the name \"{name}\".");
			}

			this.operations[name] = operation;

			return operation;
		}

		#endregion

		#region Service registration

		public void Register<TService>(Func<TService> service)
		{
			var taskInfo = typeof(Task).GetTypeInfo();
			var type = typeof(TService);
			var typeInfo = typeof(TService).GetTypeInfo();

			var serviceAttribute = typeInfo.GetCustomAttributes<ScheduledAttribute>().FirstOrDefault();

			if (serviceAttribute == null)
				throw new ArgumentException($"The type ${type} should have a ScheduledAttribute");

			foreach (var method in type.GetRuntimeMethods().Where((m) => taskInfo.IsAssignableFrom(m.ReturnType.GetTypeInfo())))
			{
				var scheduledAttribute = method.GetCustomAttribute<ScheduledAttribute>();
				if (scheduledAttribute != null)
				{
					var parameters = new List<Tuple<string,Type>>();
					foreach (var parameter in method.GetParameters())
					{
						var attribute = parameter.GetCustomAttribute<ParameterAttribute>();
						var name = attribute?.Name ?? parameter.Name;
						parameters.Add(new Tuple<string, Type>(name, parameter.ParameterType));
					}

					this.CreateMethodOperation(service, method, parameters)
					    .Save($"{serviceAttribute.Path.TrimEnd('/')}/{scheduledAttribute.Path.TrimStart('/')}");
				}
			}

		}

		private IOperation CreateMethodOperation<TService>(Func<TService> getService, MethodInfo asyncMethod, List<Tuple<string, Type>> parameters)
		{
			IOperation operation;

			Func<dynamic, CancellationToken, Task> create = (q,token) =>
			{
				// 1. Extracting all method parameters from query string 

				var dq = (q as DynamicQuery).Values;
				var args = parameters.Select((t) =>
				{
					if (t.Item2 == typeof(CancellationToken))
					{
						return token;
					}

					DynamicQuery.Parameter p;

					if (dq.TryGetValue(t.Item1, out p))
					{
						return p.To(t.Item2);
					}

					throw new ArgumentException($"Query does not contains required argument : {t.Item1}");
				});

				// 2. Invoke asynchronous method
				var service = getService();
				return asyncMethod.Invoke(service, args.ToArray()) as Task;
			};

			// Operation creation

			var returnType = asyncMethod.ReturnType.GetTypeInfo();
			if (!returnType.IsGenericType)
			{
				// A : Creating an operation without result
				operation = this.Create(create);
			}
			else
			{
				// B : If method has result, call the generic one
				var method = typeof(Operation<>);
				var genericM = method.MakeGenericType(returnType.GenericTypeArguments[0]);
				operation = Activator.CreateInstance(genericM, new object[] { this, create }) as IOperation;
			}

			// Applying modifiers from attributes to create new operations
			foreach (var behavior in asyncMethod.GetCustomAttributes<BehaviorAttribute>()) // FIXME : preserve ordering !!!
			{
				var behaviorType = behavior.GetType();
				if (behaviorType == typeof(WithRepeatAttribute))
				{
					var b = behavior as WithRepeatAttribute;
					operation = operation.WithRepeat(b.Times);
				}
				else if (behaviorType == typeof(WithUniquenessAttribute))
				{
					operation = operation.WithUniqueness();
				}

				//TODO add more behaviors
			}

			return operation;
		}

		/// <summary>
		/// Finds the operation from a given query.
		/// </summary>
		/// <returns>The operation.</returns>
		/// <param name="query">Query.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		private IOperation FindOperation(string query)
		{
			var path = query.ExtractQueryPath();

			if (!this.operations.ContainsKey(path))
			{
				throw new InvalidOperationException($"There's no operation registered with the name \"{path}\".");
			}

			return this.operations[path] as IOperation;
		}

		#endregion

		#region Execution

		public async Task<T> ExecuteAsync<T>(string query, CancellationToken token = default(CancellationToken))
		{
			if(semaphore != null) await semaphore.WaitAsync();

			var operation = this.FindOperation(query);
			var dynamicQuery = new DynamicQuery(query.ExtractQueryString());
			var task = operation.ExecuteAsync(dynamicQuery, token);
			var execution = new Execution(query, task);

			this.CurrentQueries.Add(execution);

			var r = await execution.AsTask<T>();

			this.CurrentQueries.Remove(execution);

			if (semaphore != null)  semaphore.Release();

			return r;
		}

		public Task ExecuteAsync(string query, CancellationToken token = default(CancellationToken))
		{
			return this.ExecuteAsync<Void>(query, token);
		}

		#endregion
	}
}
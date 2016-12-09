![Schema](./Doc/Logo-ld.png)

# Orkester

Lightweight framework for common centralized synchronization scenarios.

## Install

Soon on NuGet ...

## Scheduler

The Scheduler is a centralized place to request asynchronous operations from strings.

Abstraction of a request is represented easily and some behaviors can be added to registered asynchronous operations.

### Operations

An operation is nothing more than a `Task` factory for the Scheduler.

#### Creation

To instanciate operations, the easier way is `Scheduler.Create`.

##### Without result

```csharp
var operation = Scheduler.Default.Create(async (query, ct) => 
{ 
	await Task.Delay(10);
});
```

##### With result

```csharp
var operation = Scheduler.Default.Create<int>(async (query, ct) => 
{ 
	await Task.Delay(10);
	return 5;
});
```

#### Behaviors

Special behavior can be applied to operations to create new ones.

##### WithRepeat

![Schema](./Doc/WithRepeat.png)

Repeats sequentially an asynchronous functiun a number of times.


```csharp
var op = Scheduler.Default.Create<int>(async (query,ct) => 
{
	await Task.Delay(100);
	return 21;
}).WithRepeat(3); 

//->  [ 21, 21, 21 ]
```

##### WithMaxConcurrent

![Schema](./Doc/WithMaxConcurrent.png)

Executes the function with a maximum of concurent tasks at a given time.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}).WithMaxConcurrent(2);
```

##### WithLock

![Schema](./Doc/WithLock.png)

Executes the function with a maximum of one concurent task at a given time.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}).WithLock();
```

##### WithTimeout

![Schema](./Doc/WithTimeout.png)

Adds a timeout to a function execution : an exception is thrown in this case.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}).WithTimeout(Task.Delay(50));
```

##### WithUniqueness

![Schema](./Doc/WithUniqueness.png)

Executes the function only one time and caches its task for later execution requests.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}).WithUniqueness();
```

##### WithCurrent

![Schema](./Doc/WithCurrent.png)

Returns the current task in one is beeing already executed.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}.WithCurrent();
```

### WithExpiration

![Schema](./Doc/WithExpiration.png)

Returns the result of the last execution until it expires.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}).WithExpiration(100); 

```

##### WithAggregation

![Schema](./Doc/WithAggregation.png)

Wait a period before starting the task, and aggregate all requested execution during this one to return only one task.

```csharp
var op = Scheduler.Default.Create(async (query,ct) => 
{
	// ...
}).WithAggregation(100);
```

### Registration

All operations must be registred into the Scheduler to be able to be queried later. To achieve this, operation's `Save` method can be used.

```csharp
Scheduler.Default.Create<int>(async (query, ct) => 
{ 
	await Task.Delay(10);
	return 5;
}).WithUniqueness().Save("/name");
```

### Invocation

#### `Task`

```csharp
await Scheduler.Default.ExecuteAsync("/void");
```

#### `Task<T>`

```csharp
var result = await Scheduler.Default.ExecuteAsync<int>("/withresult");
```

### Queries

#### Send parameters

To pass parameters to your operation, just append a common query string to your execution query.

```csharp
var result = await Scheduler.Default.ExecuteAsync<string>("/withresult?p1=example&p2=other&p3=7&p4=true");
```

#### Access parameters

Query parameters are accessible through properties of a `dynamic` object. The parsing is made by casting the properties to one of supported types : `int`, `long`, `string`, `DateTime`, `float`, `double`, `bool`.

```csharp
Scheduler.Default.Create<string>(async (query, ct) => 
{ 
	var p1 = (string)query.p1;
	var p2 = (string)query.p2;
	var p3 = (int)query.p3;
	var p4 = (bool)query.p4;
	return $"{p1} {p2} {p3} {p4}";
}).WithUniqueness().Save("/withresult");
```

#### Signature

Query parameters values are part of operation signature : the async modifiers are independant for each set of query parameters (`?a=1&b=2` != `?a=2&b=1`, but `?a=1&b=2` == `?b=2&a=1`).

```csharp
var count = 0;

Scheduler.Default.Create(async (query, ct) => 
{ 
	await Task.Delay(10);
	count+= (int)p1 + (int)p2;
}).WithUniqueness().Save("/add");

await Scheduler.Default.ExecuteAsync("/add?p1=2&p2=1"); // + 2 + 1
await Scheduler.Default.ExecuteAsync("/add?p2=1&p1=2"); // Identical (not executed) even if unordered

await Scheduler.Default.ExecuteAsync("/add?p1=1&p2=2"); // + 1 + 2 
await Scheduler.Default.ExecuteAsync("/add?p1=1&p2=2"); // Identical

Assert.AreEqual(6,count);

```

### Services

If you have multiple operations you would like to register, you can simplify your code by declaring a service. A service is basically a class that declares asynchronous methods with special attributes for paths, params and behaviors.

```csharp
[Scheduled("/example")]
public class ExampleService
{
	public int Count { get; set; }
	
	[Scheduled("/add")]
	[WithUniqueness]
	public async Task<int> Add(int a, int b, CancellationToken token)
	{
		await Task.Delay(100,token);
		Count += a + b;
		return Count;
	}
}
```

You can then register all its operations to your scheduler.

```csharp
var service = new ExampleService();
Scheduler.Default.Register(() => service);
```

And now you're able to execute operations by querying the scheduler.

```csharp
var count = await Scheduler.Default.ExecuteAsync<int>("/example/add?a=1&b=2");
```

## Asynchronous extensions

The heart of Orkester are its set of very fluent basic extension functions for asynchronous functions. Very complex synchronization scenarios are implemented gracefuly as simple functions thanks to lambda compiled generated classes. Each one of operation behaviors can be also called from simple `Func` with extensions.

```csharp
Func<CancellationToken, Task<int>> func = async (ct) => 
{
	await Task.Delay(100);
	return 21;
};

var re = func.WithRepeat(2);

var results = await re(ct); // [ 21, 21 ]
```

## Roadmap / Ideas

* Add more extensions
* Add storage of operation results
* Improve tests

## About

Feel free to add an issue or pull request if you have any idea or found a bug.

### License

MIT © [Aloïs Deniel](http://aloisdeniel.github.io)
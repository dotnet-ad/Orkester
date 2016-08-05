# Orkester

Lightweight framework for common centralized synchronization scenarios.

## Install

Soon on NuGet ...

## Asynchronous extensions

The heart of Orkester are its very fluent basic extension function for asynchronous functions. Very complex synchronization scenarios are implemented gracefuly as simple functions thanks to lambda compiled generated classes.

### WithRepeat

![Schema](./Doc/WithRepeat.png)

Repeats sequentially an asynchronous functiun a number of times.

```csharp
Func<CancellationToken, Task<int>> func = async (ct) => 
{
	await Task.Delay(100);
	return 21;
};

var re = func.WithRepeat(2);

var results = await re(ct); // [ 21, 21 ]
```

### WithMaxConcurrent

![Schema](./Doc/WithMaxConcurrent.png)

Executes the function with a maximum of concurent tasks at a given time.

```csharp
Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
};

var co = func.WithMaxConcurrent(2); 

await Task.WhenAll(co(ct), co(ct), co(ct)); // -> 200 ms

```

### WithLock

![Schema](./Doc/WithLock.png)

Executes the function with a maximum of one concurent task at a given time.

```csharp
Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
};

var co = func.WithLock(); 

await Task.WhenAll(co(ct), co(ct), co(ct)); // -> 300 ms

```

### WithTimeout

![Schema](./Doc/WithTimeout.png)

Adds a timeout to a function execution : an exception is thrown in this case.

```csharp
Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
};

var to = func.WithTimeout(Task.Delay(50)); 

await to(ct); // -> Thrown

```

### WithUniqueness

![Schema](./Doc/WithUniqueness.png)

Executes the function only one time and caches its task for later execution requests.

```csharp
int count = 0;

Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
	count++;
};

var un = func.WithUniqueness(); 

await to(ct); // count == 1
await to(ct); // count == 1

```

### WithCurrent

![Schema](./Doc/WithCurrent.png)

Returns the current task in one is beeing already executed.

```csharp
int count = 0;

Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
	count++;
};

var cu = func.WithCurrent(); 

await Task.WhenAll(cu(ct),cu(ct)); // count == 1
await cu(ct); // count == 2

```

### WithExpiration

![Schema](./Doc/WithExpiration.png)

Returns the result of the last execution until it expires.

```csharp
int count = 0;

Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
	count++;
};

var ex = func.WithExpiration(100); 

await ex(ct); // count == 1 (-)
await ex(ct); // count == 1 (0ms)
await ex(ct); // count == 2 (100ms -> expired)

```

### WithAggregation

![Schema](./Doc/WithAggregation.png)

Wait a period before starting the task, and aggregate all requested execution during this one to return only one task.

```csharp
int count = 0;

Func<CancellationToken, Task> func = async (ct) => 
{
	await Task.Delay(100);
	count++;
};

var ex = func.WithAggregation(100); 

var t1 = ex(ct);
await Task.Delay(50);
var t2 = ex(ct);
await Task.Delay(100);
var t3 = ex(ct);

await Task.WhenAll(t1,t2,t3); // count == 2

```

## Scheduler

The Scheduler adds a centralized place to request asynchronous operations from string queries.

Abstraction of a request is represented easily as query and each asynchronous extension can be used.


### Registration

#### `Task`

```csharp
Scheduler.Default.Create(async (query, ct) => 
{ 
	await Task.Delay(10);
}).Save("/void");
```

#### `Task<T>`

```csharp
Scheduler.Default.Create<int>(async (query, ct) => 
{ 
	await Task.Delay(10);
	return 5;
}).Save("/withresult");
```

#### Async extensions

```csharp
Scheduler.Default.Create<int>(async (query, ct) => 
{ 
	await Task.Delay(10);
	return 5;
}).WithUniqueness().Save("/unique");
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

## Roadmap / Ideas

* Add more extensions
* Add storage of operation results
* Improve tests

## About

Feel free to add an issue or pull request if you have any idea or bug.

### License

MIT © [Aloïs Deniel](http://aloisdeniel.github.io)
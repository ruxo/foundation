# RZ Functional library for C#

## Option[T] (RZ.Foundation) ##

There are two states for `Option<T>` type: `Some` or `None`.

### Create Some value ###

Recommendation is to use `Prelude` module.

```c#
using static RZ.Foundation.Prelude;

Option<int> intOption = Some(123);

// alternatives
var intOption1 = 123.ToOption();
var intOption2 = Option<int>.Some(123);
var intOption3 = (Option<int>) 123;
Option<int> intOption4 = 123;
var intOption5 = Option<int>.From(123); // Some
```

### Create None value ###

```c#
using static RZ.Foundation.Prelude;

// recommended
Option<int> intOption = None<int>();

// alternatives
var intOption1 = Option<int>.None();
var intOption2 = Option<int>.From(null);
```

### Convert between C# nullable value and Option[T] ##

Prelude module provides conversion functions for C# nullable and Option[T]. 
Unfortunately, Value Type/Ref Type semantic differences make creating generic functions to
support both meta-types tedious and, in some case, confuse the compiler and generating
a warning. 

```c#
using static RZ.Foundation.Prelude;

int? x = 123;
Option<T> y = Some(x);  // convert a nullable value to Option<T>
int? z = y.ToNullable(); // convert an Option<T> to a nullable value

// this should also work with nullable ref value
string? a = "Hello";
Option<string> b = Some(a);
string? c = b.ToNullable();
```

### Getting a value out of Option[T] ###

Suppose that we have follow option values:

```c#
using static RZ.Foundation.Prelude;

var some = Some(123);
var none = None<int>();
```

#### Get ####

```c#
var x = some.Get();     // 123
var y = none.Get();     // an exception is thrown!

// alternatives: map & get
var a = some.Get(v => v + 1, () => 999);    // 124
var b = none.Get(v => v + 1, () => 999);    // 999

// async map & get
var c = await some.GetAsync(v => Task.FromResult(v+1), () => Task.FromResult(999)); // 124
var d = await some.GetAsync(v => Task.FromResult(v+1), () => Task.FromResult(999)); // 999
```

#### GetOrThrow ####

In case you want to specific an exception.

```c#
var x = some.GetOrThrow(() => new MyException());     // 123
var y = none.GetOrThrow(() => new MyException());     // MyException is thrown!
```

#### GetOrDefault ####

```c#
var x = some.GetOrDefault();   // 123
var y = none.GetOrDefault();   // 0, int's default value
var z = none.GetOrDefault(999); // 999
```

#### GetOrElse ####

```c#
var x = some.GetOrElse(999);    // 123
var y = none.GetOrElse(999);    // 999

var u = some.GetOrElse(() => 999);  // 123
var v = none.GetOrElse(() => 999);  // 999

var a = await some.GetOrElseAsync(() => Task.FromResult(999));  // 123
var b = await none.GetOrElseAsync(() => Task.FromResult(999));  // 999 
```

### Mapping ###

Mapping over `Option[T]` only applies on `Some` value. 

```c#
using static RZ.Foundation.Prelude;

Option<int> x = Some(100);
Option<string> y = x.Map(value => value.ToString());
Option<string> z = await x.MapAsync(value => Task.FromResult(value.ToString()));
```

### Chain/Bind ###

Chaining is like mapping, specific map to an `Option[T]`.

```c#
using static RZ.Foundation.Prelude;
var x = Some(100);
var y = x.Chain(v => v > 100? None<int>() : Some(v + 23));
var z = await x.ChainAsync(v => Task.FromResult(v > 100? None<int>() : Some(v+23)));
```

### Replace None value ###

```c#
using static RZ.Foundation.Prelude;
var x = None<int>();
var y = x.OrElse(123);  // Option<int> of 123
var z = x.OrElse(Some(456)) // Option<int> of 456
var a = x.OrElse(() => None<int>());    // stay None
var b = await x.OrElseAsync(() => Task.FromResult(999));    // Option<int> of 999
```

### Perform action (Side effect) ###

`Then` and `IfNone` are operations to perform action.  These methods return the original value.

```c#
using static RZ.Foundation.Prelude;
var x = None<int>();
x.Then(v => Console.WriteLine("Value = {0}", v));
x.IfNone(() => Console.WriteLine("No value"));
x.Then(v => Console.WriteLine("Value = {0}", v), () => Console.WriteLine("No value"));

// async versions
await x.ThenAsync(v => Task.Run(() => Console.WriteLine("V = {0}", v)));
await x.IfNoneAsync(() => Task.Run(() => Console.WriteLine("No value")));
await x.ThenAsync(v => Task.Run(() => Console.WriteLine("V = {0}", v)),
                  () => Task.Run(() => Console.WriteLine("No value")));
```

### Type casting ###

```c#
Option<object> x = Some((object) "I am string");
Option<string> y = x.TryCast<string>();
Option<int> z = x.TryCast<int>();   // None!
```
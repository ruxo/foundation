# RZ Functional library for C#

This library is an add-on to LanguageExt library. It tries to provide some 
syntactic sugar for more natural expression.

## Option[T] Extension (RZ.Foundation) ##

### Convert between C# nullable value and Option[T] ##

Prelude module provides conversion functions for C# nullable and Option[T]. 
Unfortunately, Value Type/Ref Type semantic differences make creating generic functions to
support both meta-types tedious and, in some case, confuse the compiler and generating
a warning. 

```c#
using static LanguageExt.Prelude;

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
using static LanguageExt.Prelude;

var some = Some(123);
var none = Option<int>.None;
```

#### Get ####

```c#
var x = some.Get();     // 123
var y = none.Get();     // an exception is thrown!
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
```

### Replace None value ###

```c#
using static LanguageExt.Prelude;
var x = Option<int>.None;
var y = x.OrElse(123);  // Option<int> of 123
var z = x.OrElse(Some(456)) // Option<int> of 456
var a = x.OrElse(() => None);    // stay None
var b = await x.OrElseAsync(() => Task.FromResult(999));    // Option<int> of 999
```

### Perform action (Side effect) ###

`Then` is the operation to perform an action. The method returns the original value.

```c#
using static LanguageExt.Prelude;
var x = None<int>();
x.Then(v => Console.WriteLine("Value = {0}", v));
x.Then(v => Console.WriteLine("Value = {0}", v), () => Console.WriteLine("No value"));

// async versions
await x.ThenAsync(v => Task.Run(() => Console.WriteLine("V = {0}", v)));
await x.ThenAsync(v => Task.Run(() => Console.WriteLine("V = {0}", v)),
                  () => Task.Run(() => Console.WriteLine("No value")));
```

### Type casting ###

```c#
Option<object> x = Some((object) "I am string");
Option<string> y = x.TryCast<object,string>();
Option<int> z = x.TryCast<object,int>();   // None!
```

## TaskOption[T] ##

LanguageExt's `OptionAsync` should be a wrapper of `Task<Option<T>>`, but its recent async/await handler
has been implemented in way that consumes all exceptions as `None` value. This makes sense when we don’t
want any side-effect. But in case of exception handling, I find that by allowing exceptions as the side-effect,
would simplify error handling code when writing in functional paradigm.

So `TaskOption<T>` is made to work similar to `OptionAsync` but with async/await pattern that allows
exceptions to be escalated normally, as well as, support `None` returning value.

## Nullable as Option like

```c#
int? x = 123;
string MyToString(int a) => a.ToString();
string? y = x.Apply(MyToString);    // "123"
```
# RZ.Foundation

RZ.Foundation is a functional add-on to [LanguageExt](https://github.com/louthy/language-ext).
Its centrepiece is **`Outcome<T>`**: a result type that brings Go's *"errors are values"*
philosophy to C#. Instead of throwing exceptions and hoping someone, somewhere, remembers to
`catch`, a function simply **returns** either its value or a structured **`ErrorInfo`** — and
the compiler keeps you honest about which one you got.

```csharp
// The signature tells the whole story: this can fail, and you must deal with it.
Outcome<User> FindUser(string id);
```

## Why `Outcome<T>`?

In the exception model, the failure path is invisible. A method that returns `User` *looks*
total, but may blow up the call stack at runtime. You only discover the failure modes by
reading the implementation — or in production.

`Outcome<T>` makes failure a first-class, visible part of the return value:

- **Explicit** — failure is in the type signature, not hidden in a comment.
- **Local** — you handle (or deliberately forward) the error right where it happens.
- **Cheap** — no stack unwinding on the expected-error path; exceptions are reserved for the
  genuinely exceptional and for system boundaries.

### Go vs. C#, side by side

```go
// Go
v, err := DoThing()
if err != nil {
    return err          // forward the error upward
}
use(v)
```

```csharp
// C# with RZ.Foundation
if (Fail(DoThing(), out var e, out var v))
    return e.Trace();   // forward the error upward (and record this hop — see below)
Use(v);
```

Same shape, same intent. The C# side gets something extra for free: calling `e.Trace()`
stamps the current file/method/line onto the error as it travels up, so by the time it
surfaces you have a readable, application-level breadcrumb trail.

> All examples below assume the project's global usings are in effect:
> `using static RZ.Foundation.AOT.Prelude;` and `using static RZ.Foundation.StandardErrorCodes;`.
> `Outcome<T>` lives in `RZ.Foundation`; `ErrorInfo` in `RZ.Foundation.Types`.

## `Outcome<T>` in 60 seconds

An `Outcome<T>` is either a **success** carrying a `T` or a **failure** carrying an
`ErrorInfo`. You rarely construct it explicitly — values and errors **implicitly convert**:

```csharp
Outcome<int>  ok  = 42;                              // success, by implicit conversion
Outcome<int>  bad = new ErrorInfo(NotFound, "no such row");  // failure, by implicit conversion
Outcome<int>  bad2 = ErrorInfo.New(NotFound, "no such row");  // failure, by a helper function with location attached

// Explicit factories when type inference needs a nudge:
var a = SuccessOutcome(42);            // Outcome<int> success
var b = FailedOutcome<int>(ErrorInfo.NotFound);
var u = UnitOutcome;                   // Outcome<Unit>, the "succeeded, nothing to return" case
```

A function returning `Outcome<T>` therefore reads naturally — just `return value;` or
`return error;`:

```csharp
Outcome<int> Divide(int a, int b)
    => b == 0 ? new ErrorInfo(InvalidRequest, "division by zero") : a / b;
```

## `ErrorInfo` — the structured replacement for `Exception`

`ErrorInfo` is what `Outcome<T>` carries on failure. It is an immutable record designed to
describe an error well enough that you never need an exception type per failure mode.

| Field        | Purpose                                                        |
|--------------|----------------------------------------------------------------|
| `Code`       | Machine-readable category, e.g. `"not-found"`                  |
| `Message`    | Human-readable description                                     |
| `TraceId`    | Distributed-trace id, auto-filled from `Activity.Current`      |
| `DebugInfo`  | Extra developer detail (omit in release responses)            |
| `Data`       | Serialized payload associated with the error                  |
| `Locations`  | The application-side call trail accumulated by `Trace()`      |

Use the well-known codes from `StandardErrorCodes` (`NotFound`, `InvalidRequest`, `Timeout`,
`Duplication`, `HttpError`, `Unhandled`, `ValidationFailed`, …) rather than inventing strings:

```csharp
var notFound = ErrorInfo.New(NotFound, "user 42 does not exist");
var invalid  = new ErrorInfo(InvalidRequest, "email is required");

// Inspect:
notFound.Is(NotFound);        // true
notFound.IsNotFound();        // true  — shorthand for the common case

// Re-tag a low-level error with higher-level context:
var wrapped = dbError.Wrap(ServiceError, "failed to load user profile");
```

## Synchronous flow

The everyday pattern is the **guard**: pull the value out, or short-circuit. Two mirror-image
helpers cover it.

`Success(...)` is true on the happy path:

```csharp
if (Success(FindUser(id), out var user, out var error))
    Render(user);
else
    Log(error);
```

`Fail(...)` is true on the failure path — perfect for early-return propagation:

```csharp
Outcome<Invoice> BuildInvoice(string userId, string itemId) {
    if (Fail(FindUser(userId), out var e, out var user)) return e.Trace();
    if (Fail(FindItem(itemId), out e,  out var item)) return e.Trace();

    return new Invoice(user, item);   // implicit conversion to Outcome<Invoice>
}
```

> **Always forward with `return e.Trace();`, not bare `return e;`.**
> `Trace()` appends the current file/method/line to the error's `Locations`. As the error
> bubbles through `BuildInvoice` → its caller → *its* caller, each hop leaves a marker,
> giving you an application-level trace that reads in your own terms — far more useful than a
> raw CLR stack trace, and it costs nothing on the success path.

There are convenient overloads. Take only what you need:

```csharp
if (Fail(result, out var e)) return e.Trace();   // don't care about the value
if (Success(result, out var v)) Use(v);          // don't care about the error
```

Tuple results deconstruct in one step:

```csharp
if (Fail(LoadPair(), out var e, out var first, out var second)) return e.Trace();
// first and second are in scope here
```

To collapse an `Outcome<T>` into a plain value, use `Match` or `IfFail`:

```csharp
var label = FindUser(id).Match(u => u.Name, _ => "(unknown)");
var count = CountRows().IfFail(0);          // default on failure
var count2 = CountRows().IfFail(e => -1);   // compute a default from the error
```

## Asynchronous flow

Async code returns `ValueTask<Outcome<T>>`. The pattern is identical — `await` first, then
guard exactly as in the synchronous case:

```csharp
async ValueTask<Outcome<Profile>> LoadProfile(string id) {
    if (Fail(await FetchUser(id), out var e, out var user)) return e.Trace();
    if (Fail(await FetchAvatar(user), out e, out var avatar)) return e.Trace();

    return new Profile(user, avatar);
}
```

When the work might throw (an HTTP call, a DB driver), wrap it with `TryCatch` to turn an
exception into a failed `Outcome` instead of letting it escape (see
[Bridging with exceptions](#bridging-with-exceptions) for the full set):

```csharp
ValueTask<Outcome<string>> GetBody(Uri url) =>
    TryCatch(async () => await httpClient.GetStringAsync(url));
// a thrown HttpRequestException becomes a failed Outcome<string>
```

## The `NotFound` convention

There is one error code the library treats specially: **`NotFound`**. It exists for a common
situation — a lookup that may legitimately find *nothing*, where "nothing" is an ordinary
outcome rather than a real failure.

You could model that with `Outcome<Option<T>>`: success-with-`Some`, success-with-`None`, or
failure. It is precise, but every caller now has to unwrap two layers. The lighter alternative
is to return a plain `Outcome<T>` and signal absence with a `NotFound` failure:

```csharp
// Returns the user, or a NotFound failure if there is no such id.
Outcome<User> FindUser(string id)
    => TryLookup(id) is { } user ? user : ErrorInfo.NotFound;
```

Callers that don't care about the distinction just forward like any other error. Callers that
*do* care get purpose-built helpers so "missing" never gets mistaken for "broken":

```csharp
// Forward real errors, but let "not found" fall through to a fallback.
if (FailButNotFound(FindUser(id), out var e, out var user)) return e.Trace();
var effective = user ?? User.Guest;       // not-found ⇒ user is null here

// Or substitute a fallback inline — only not-found is replaced; real errors pass through:
Outcome<User> u  = FindUser(id).IfNotFound(User.Guest);
Outcome<User> u2 = FindUser(id).IfNotFound(() => LoadDefault());

// Test it directly:
if (FindUser(id).IsNotFound()) ...

// When a caller genuinely wants the Option<T> shape back, recover it:
Outcome<Option<User>> maybe = FindUser(id).CheckNotFound();   // not-found ⇒ success(None)

// At an exception boundary, not-found becomes null instead of throwing:
User? user = await ThrowUnlessNotFound(FindUserAsync(id));
```

> **Pitfall — document the behaviour.** Returning `NotFound` from an `Outcome<T>` is exactly
> like `List.FindIndex` returning `-1`: a useful shortcut that is invisible from the signature.
> If a function uses `NotFound` as a normal, expected result, **say so in its doc comment** —
> otherwise a caller will treat the absence as an error and propagate it. Reach for
> `Outcome<Option<T>>` instead when the distinction must be impossible to miss.

## Streaming with `IAsyncEnumerable<Outcome<T>>`

A risky async stream can be turned into a stream of outcomes with `TryCatch`, which wraps each
item and emits a trailing failure item if iteration throws:

```csharp
IAsyncEnumerable<Outcome<Row>> rows = TryCatch(ReadRowsAsync(query));
```

To consume the whole stream into a list, use **`MakeList`** (read-only) or **`MakeMutableList`**
(a `List<T>`). Both **short-circuit on the first failure** — you get the value list, or the
first error, never a half-built list paired with a swallowed exception:

```csharp
// Outcome<IReadOnlyList<Row>> — all rows, or the first error encountered
if (Fail(await rows.MakeList(), out var e, out var allRows)) return e.Trace();
Process(allRows);

// MakeMutableList when you need to keep appending afterwards:
if (Fail(await TryCatch(ReadRowsAsync(query)).MakeMutableList(), out var e2, out var list))
    return e2.Trace();
list.Add(extraRow);
```

Both have a **selector** overload that projects each item while collecting:

```csharp
// Outcome<IReadOnlyList<string>>
var names = await TryCatch(ReadUsersAsync()).MakeList(u => u.Name);
```

> Note: if the source is known to be empty, `MakeList` / `MakeMutableList` return
> `ErrorInfo.NotFound` rather than an empty list — handle it with `FailButNotFound` or
> `IfNotFound` if "no rows" is acceptable for your case.

The same extension set also offers `First`, `Last`, `Average`, and `AverageBy`, each returning
an `Outcome<T>` that short-circuits on the first failing item.

## Bridging with exceptions

`Outcome<T>` interoperates cleanly with exception-based code at both ends.

**Pulling exceptions in.** `TryCatch` (sync and async) converts a throwing call into an
`Outcome`; `Try` returns a plain `(Exception?, T)` tuple you can convert later:

```csharp
Outcome<int> parsed = TryCatch(() => int.Parse(input));      // sync
```

**Pushing errors out.** At an API boundary where callers expect exceptions, unwrap:

```csharp
var user = FindUser(id).Unwrap();                 // throws ErrorInfoException if failed
var user2 = await ThrowIfError(LoadProfile(id));  // same, for ValueTask<Outcome<T>>
var maybe = await ThrowUnlessNotFound(Load(id));  // throws on real errors; null on not-found
```

**Custom exceptions → codes.** Tag an exception type with `[ErrorInfo]` and `ErrorFrom`
will map it to the right `Code` automatically; otherwise it becomes `Unhandled`:

```csharp
[ErrorInfo(Duplication)]
sealed class DuplicateKeyException(string msg) : Exception(msg);

// Anywhere a caught exception needs to become an ErrorInfo:
ErrorInfo info = ErrorFrom.Exception(caughtException);   // honours [ErrorInfo]
ErrorInfo prog = ErrorFrom.Program("invalid state");     // quick InvalidRequest with caller name
```

`ErrorInfoException` is the bridge type: it carries `Code` and `DebugInfo`, and round-trips
via `.ToErrorInfo()`.

## Composing outcomes (Experimental)

`Outcome<T>` supports the usual functional combinators:

```csharp
var len = FindUser(id).Map(u => u.Name.Length);
var inv = FindUser(id).Bind(u => BuildInvoice(u.Id, itemId));
```

It also offers LINQ query syntax (`from … from … select`), including an async form over
`ValueTask<Outcome<T>>`:

```csharp
var result = from u in FindUser(id)
             from i in FindItem(itemId)
             select new Invoice(u, i);
```

> ⚠️ **Caution: the LINQ form is not reliable for `Outcome<T>` — prefer the guard style.**
> The query syntax compiles into nested `SelectMany`/`Select` closures and, in the async case,
> a generated state machine. In practice this has broken under code that participates in
> ambient, flow-sensitive state — for example operations running inside a **MongoDB
> transaction**, where the LINQ-generated state machine appears to disrupt the transaction
> context and the call fails. Until this is understood and fixed, treat LINQ over `Outcome<T>`
> as experimental: use the `Success`/`Fail` guard pattern (above) for anything inside a
> transaction or other context-sensitive scope.

## Helper cheat-sheet

| Need                                    | Use                                            |
|-----------------------------------------|------------------------------------------------|
| Make a success / failure                | `SuccessOutcome(v)` / `FailedOutcome<T>(e)` (or just `return v;` / `return new ErrorInfo(code, msg);`) |
| Happy-path guard                        | `if (Success(x, out var v, out var e))`        |
| Forward an error early                  | `if (Fail(x, out var e, out var v)) return e.Trace();` |
| Treat not-found as non-error            | `FailButNotFound(x, out var e, out var v)`     |
| Turn a throwing call into an `Outcome`  | `TryCatch(() => ...)` / `TryCatch(async ...)`  |
| Collapse to a value                     | `x.Match(onOk, onErr)` / `x.IfFail(default)`   |
| Exit to exception-based code            | `x.Unwrap()` / `await ThrowIfError(task)`      |
| Collect an async stream                 | `await TryCatch(stream).MakeList()` / `.MakeMutableList()` |
| Build a structured error                | `ErrorInfo.New(code, message)` / `StandardErrorCodes` |
| Add app-level trace while propagating   | `error.Trace()`                                |
| Wrap a low-level error in context       | `error.Wrap(code, message)`                    |

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RZ.Foundation is a functional programming library for C#, built as an add-on to LanguageExt. It provides syntactic sugar for composable error handling (Outcome/ErrorInfo), Option extensions, async helpers, and JSON serialization for functional types. Published as a NuGet package.

## Build & Test Commands

```bash
# Build the entire solution
dotnet build RZ.Foundation.slnx

# Run all tests
dotnet test RZ.Foundation.slnx

# Run a single test by name (TUnit uses --filter, xUnit uses --filter)
dotnet test test/RZ.Foundation.AOT.Test --filter "OutcomeDirectSuccessAssignment"
dotnet test test/RZ.Foundation.Test --filter "FullyQualifiedName~SomeTestName"

# Create NuGet packages (PowerShell)
./build.ps1 <destination-path>
```

## Solution Structure

- **`src/RZ.Foundation.AOT`** — AOT-compatible core library. Contains functional types (Outcome, ErrorInfo), extensions, helpers. Marked `IsAotCompatible=true` with JSON reflection disabled (uses source generators).
- **`src/RZ.Foundation`** — Main library referencing AOT project. Adds JSON converters for LanguageExt types (Option, Seq, Set, Map) and HTTP extensions. This is the published NuGet package.
- **`test/RZ.Foundation.AOT.Test`** — Tests using **TUnit** framework with `Assert.That(...).IsTrue()` style assertions.
- **`test/RZ.Foundation.Test`** — Tests using **xUnit.v3** with **FluentAssertions**.

## Key Technical Details

- **Target framework:** .NET 10.0, C# preview (`LangVersion=preview`)
- **Primary dependency:** LanguageExt.Core 4.4.9
- **Versioning:** MinVer (automatic semver from git tags)
- **Test runner:** Microsoft.Testing.Platform (configured in `global.json`)
- **Solution format:** `.slnx` (modern XML format)

## Architecture

### Core Types (in RZ.Foundation.AOT)

- **`Outcome<T>`** — The primary result type. Represents success (with data) or failure (with `ErrorInfo`). Supports monadic composition via `Bind`, `Map`, `Match`, and pipe operator `|` for error-handling chains (`@catch`, `failDo`, `@do`).
- **`ErrorInfo`** — Structured error with Code, Message, TraceId, DebugInfo, inner/sub errors, and stack trace. Integrates with OpenTelemetry `Activity.Current` for distributed tracing.
- **`ErrorInfoException`** — Bridges functional error handling with exception-based code.
- **`Prelude` (static class)** — Re-exports LanguageExt functions (`Some`, `None`, `Seq`, `Optional`) and adds `Try`, `TryCatch`, `On`, `@catch`, `failDo`, `@do` for railway-oriented programming.
- **`StandardErrorCodes`** — Constants like `NotFound`, `InvalidRequest`, `Timeout`, etc.

### Global Usings Convention

Both `src/RZ.Foundation.AOT/GlobalUsings.cs` and `src/CommonGlobalUsings.cs` import LanguageExt, Prelude, StandardErrorCodes, and JetBrains.Annotations globally. New source files in these projects get these automatically.

### JSON Serialization (in RZ.Foundation)

Custom `System.Text.Json` converters for LanguageExt types: `OptionJsonConverter`, `OutcomeConverter`, `SeqJsonConverter`, `SetJsonConverter`, `MapJsonConverter`. Uses `RzJsonDerivedTypeAttribute` for polymorphic type deserialization. AOT project uses source-generated `JsonSerializerContext` (no reflection).

### Patterns

- **Railway-oriented programming:** Chain operations with `Bind`/`Map`, handle errors with `| @catch(handler) | failDo(logError)`.
- **TryCatch:** Convert exceptions to `Outcome<T>` via `TryCatch(() => riskyOp())` (sync and async variants).
- **On handler:** `On(task).Catch(handler).BeforeThrow(action)` for fluent exception handling.
- **Side effects:** `failDo()` / `@do()` perform actions while preserving the original value.

## Testing Conventions

- AOT tests use TUnit: `[Test]` attribute, `async ValueTask` return type, `await Assert.That(x).IsEqualTo(y)`.
- Foundation tests use xUnit.v3: `[Fact]`/`[Theory]` attributes, FluentAssertions (`x.Should().Be(y)`).
- Test namespaces mirror source namespaces under `RZ.Foundation.Test`.
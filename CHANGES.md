# Version changes

## 7.0.0

* Introduce two new libraries: `RZ.Foundation.Blazor` and `RZ.Foundation.Blazor.MudBlazor`
* Improve `Outcome` types
  * Support `guard` and `guardnot`
  * Support `BiMap`
  * Introduce `Use` for disposable cleanup
  * Remove redundant `SelectMany`

## 6.5.0

* Introduce new `OutcomeT`, `Synchronous` IO, and `Asynchronous` IO. Also using "typeclass" style
  which is introduced by the new LanguageExt library v5.
* (Major break) `OutcomeAsync` is removed! Use `OutcomeT` instead.
* (Major break) `TypeOption` is removed! I believe noone uses it.

## 6.4.0

* Introduce `Outcome` and `OutcomeAsync`, as an experimental feature.
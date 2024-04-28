# Version changes

## 6.5.0

* Introduce new `OutcomeT`, `Synchronous` IO, and `Asynchronous` IO. Also using "typeclass" style
  which is introduced by the new LanguageExt library v5.
* (Major break) `OutcomeAsync` is removed! Use `OutcomeT` instead.
* (Major break) `TypeOption` is removed! I believe noone uses it.

## 6.4.0

* Introduce `Outcome` and `OutcomeAsync`, as an experimental feature.
# Funca.Abstractions

Abstractions for functional .NET applications, including `Result<T>` and `Option<T>` containers for Railway-Oriented Programming.

## Target framework

This package targets .NET 10 LTS.

## Result invariants

Every failed `Result<T>` carries at least one non-null `ErrorResult`. Error collections are copied on construction and when materialized, so callers cannot mutate a result after it has been created.

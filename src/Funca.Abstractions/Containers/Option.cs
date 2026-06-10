namespace Funca.Abstractions.Containers;

public readonly record struct Option<T>
{
    public T? Value { get; init; }

    public bool IsSome => Value is not null;

    public bool IsNone => !IsSome;

    public static Option<T> Some(T value) => value is null
        ? throw new ArgumentNullException(nameof(value))
        : new Option<T> { Value = value };

    public static Option<T> None() => new();

    public T Unwrap() => IsNone ? throw new InvalidOperationException("Option has no value.") : Value!;

    public T UnwrapOr(T fallback) => IsSome ? Value! : fallback;

    public T? UnwrapOrDefault() => IsSome ? Value : default;

    public override string ToString() => IsSome
        ? $"Some({Value})"
        : "None";
}
namespace Funca.Abstractions.Containers;

public readonly record struct Option<T>(T? Value)
{
    public bool IsSome { get; } = Value is not null;

    public bool IsNone => !IsSome;

    public static Option<T> Some(T value) => value is null
        ? throw new InvalidOperationException(nameof(value))
        : new Option<T>(value);

    public static Option<T> None() => new(default);

    public override string ToString() => IsSome
        ? $"Some({Value})"
        : "None";
}
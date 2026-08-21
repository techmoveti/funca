namespace Funca.Abstractions.Containers;

public readonly record struct Option<T>
{
    private readonly bool _isSome;

    public T? Value { get; }

    public bool IsSome => _isSome;

    public bool IsNone => !IsSome;

    public static Option<T> Some(T value) => value is null
        ? throw new ArgumentNullException(nameof(value))
        : new Option<T>(value, true);

    public static Option<T> None() => new(default, false);

    private Option(T? value, bool isSome)
    {
        Value = value;
        _isSome = isSome;
    }

    public T Unwrap() => IsNone ? throw new InvalidOperationException("Option has no value.") : Value!;

    public T UnwrapOr(T fallback) => IsSome ? Value! : fallback;

    public T? UnwrapOrDefault() => IsSome ? Value : default;

    public override string ToString() => IsSome
        ? $"Some({Value})"
        : "None";
}

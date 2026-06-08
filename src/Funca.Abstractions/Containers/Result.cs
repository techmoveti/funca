namespace Funca.Abstractions.Containers;

public readonly record struct Result<T>(
    T? Value,
    ErrorResult[] Errors)
{
    public bool IsOk { get; } = Value is not null;
    public bool IsError { get; } = Value is null;

    public static Result<T> Wrap(T? value) => new(value, []);

    public T Unwrap() => Value ?? throw new ArgumentNullException(nameof(Value));

    public static implicit operator Result<T>(ErrorResult error) => new(default, [error]);

    public static implicit operator Result<T>(ErrorResult[] errors) => new(default, errors);
}
namespace Funca.Abstractions.Containers;

public readonly record struct Result<T>
{
    public T? Value { get; init; }

    public ErrorResult[] Errors => field ?? Array.Empty<ErrorResult>();

    public bool IsOk => Errors.Length == 0 && Value is not null;

    public bool IsError => !IsOk;

    public bool HasErrors => Errors.Length > 0;

    private Result(T? value, ErrorResult[]? errors)
    {
        Value = value;
        Errors = errors;
    }

    public static Result<T> Wrap(T? value) => new(value, Array.Empty<ErrorResult>());

    public static Result<T> Error(ErrorResult error) => new(default, [error]);

    public static Result<T> Error(ErrorResult[] errors) => new(default, errors ?? Array.Empty<ErrorResult>());

    public T Unwrap()
        => !IsOk ? throw new InvalidOperationException("Result does not contain a success value.") : Value!;

    public T UnwrapOr(T fallback) => IsOk ? Value! : fallback;

    public T? UnwrapOrDefault() => IsOk ? Value : default;

    public override string ToString()
        => IsOk
            ? $"Ok({Value})"
            : $"Error[{string.Join(", ", Errors)}]";

    public static implicit operator Result<T>(ErrorResult error) => Error(error);

    public static implicit operator Result<T>(ErrorResult[] errors) => Error(errors);
}
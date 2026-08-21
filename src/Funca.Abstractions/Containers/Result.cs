namespace Funca.Abstractions.Containers;

public readonly record struct Result<T>
{
    public T? Value { get; init; }

    private readonly ErrorResult? _singleError;
    private readonly ErrorResult[]? _manyErrors;

    public bool HasErrors => _singleError is not null || (_manyErrors?.Length > 0);

    public bool IsOk => !HasErrors && Value is not null;

    public bool IsError => !IsOk;

    /// <summary>
    /// Returns the errors as a <see cref="ReadOnlySpan{T}"/> — allocation-free for 0 and 1 error cases.
    /// </summary>
    public ReadOnlySpan<ErrorResult> Errors
    {
        get
        {
            if (_manyErrors is { Length: > 0 }) return _manyErrors;
            if (_singleError is not null) return new[] { _singleError };
            return [];
        }
    }

    private Result(T? value, ErrorResult? singleError, ErrorResult[]? manyErrors)
    {
        Value = value;
        _singleError = singleError;
        _manyErrors = manyErrors;
    }

    public static Result<T> Wrap(T? value) => new(value, null, null);

    public static Result<T> Error(ErrorResult error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, error, null);
    }

    public static Result<T> Error(ErrorResult[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return errors.Length switch
        {
            0 => new(default, null, null),
            1 => new(default, errors[0], null),
            _ => new(default, null, errors)
        };
    }

    public static Result<T> Error(ReadOnlySpan<ErrorResult> errors)
    {
        if (errors.IsEmpty) return new(default, null, null);
        if (errors.Length == 1) return new(default, errors[0], null);
        return new(default, null, errors.ToArray());
    }

    /// <summary>
    /// Materializes errors into an array. Prefer <see cref="Errors"/> (ReadOnlySpan) when possible
    /// to avoid the allocation in the single-error case.
    /// </summary>
    public ErrorResult[] ErrorsToArray()
    {
        if (_manyErrors is { Length: > 0 }) return _manyErrors;
        if (_singleError is not null) return [_singleError];
        return [];
    }

    public T Unwrap()
        => !IsOk ? throw new InvalidOperationException("Result does not contain a success value.") : Value!;

    public T UnwrapOr(T fallback) => IsOk ? Value! : fallback;

    public T? UnwrapOrDefault() => IsOk ? Value : default;

    public override string ToString()
        => IsOk
            ? $"Ok({Value})"
            : $"Error[{string.Join(", ", ErrorsToArray())}]";

    public static implicit operator Result<T>(ErrorResult error) => Error(error);

    public static implicit operator Result<T>(ErrorResult[] errors) => Error(errors);
}
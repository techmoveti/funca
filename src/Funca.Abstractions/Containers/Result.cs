namespace Funca.Abstractions.Containers;

public readonly record struct Result<T>
{
    public T? Value { get; init; }

    private readonly bool _isSuccess;
    private readonly ErrorResult? _singleError;
    private readonly ErrorResult[]? _manyErrors;

    public bool HasErrors => _singleError is not null || (_manyErrors?.Length > 0);

    /// <summary>
    /// Returns <c>true</c> when this result represents a successful operation.
    /// Success is tracked by an explicit flag and is independent of whether <typeparamref name="T"/>
    /// is nullable or whether <see cref="Value"/> is <c>null</c>.
    /// </summary>
    public bool IsOk => _isSuccess;

    public bool IsError => !_isSuccess;

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

    private Result(T? value, bool isSuccess, ErrorResult? singleError, ErrorResult[]? manyErrors)
    {
        Value = value;
        _isSuccess = isSuccess;
        _singleError = singleError;
        _manyErrors = manyErrors;
    }

    /// <summary>
    /// Creates a successful result. <paramref name="value"/> may be <c>null</c> when
    /// <typeparamref name="T"/> is a nullable type — success is tracked independently.
    /// </summary>
    public static Result<T> Wrap(T? value) => new(value, true, null, null);

    public static Result<T> Error(ErrorResult error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, false, error, null);
    }

    public static Result<T> Error(ErrorResult[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return errors.Length switch
        {
            0 => new(default, false, null, null),
            1 => new(default, false, errors[0], null),
            _ => new(default, false, null, errors)
        };
    }

    public static Result<T> Error(ReadOnlySpan<ErrorResult> errors)
    {
        if (errors.IsEmpty) return new(default, false, null, null);
        if (errors.Length == 1) return new(default, false, errors[0], null);
        return new(default, false, null, errors.ToArray());
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
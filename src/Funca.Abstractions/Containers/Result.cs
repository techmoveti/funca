namespace Funca.Abstractions.Containers;

public readonly record struct Result<T>
{
    // ReSharper disable once StaticMemberInGenericType
    static private readonly ErrorResult[] UninitializedErrors =
        [ErrorResult.Failure("Result was not initialized.")];

    internal readonly T? Value;
    private readonly bool _isSuccess;
    private readonly ErrorResult[]? _errors;

    public bool HasErrors => IsError;

    /// <summary>
    /// Returns <c>true</c> when this result represents a successful operation.
    /// Success is tracked by an explicit flag and is independent of whether <typeparamref name="T"/>
    /// is nullable or whether <see cref="Value"/> is <c>null</c>.
    /// </summary>
    public bool IsOk => _isSuccess;

    public bool IsError => !_isSuccess;

    /// <summary>
    /// Returns a read-only view of the errors without exposing the backing storage.
    /// </summary>
    public ReadOnlySpan<ErrorResult> Errors => IsOk
        ? []
        : _errors ?? UninitializedErrors;

    private Result(T? value, bool isSuccess, ErrorResult[]? errors)
    {
        Value = value;
        _isSuccess = isSuccess;
        _errors = errors;
    }

    /// <summary>
    /// Creates a successful result. <paramref name="value"/> may be <c>null</c> when
    /// <typeparamref name="T"/> is a nullable type — success is tracked independently.
    /// </summary>
    public static Result<T> Wrap(T? value) => new(value, true, null);

    public static Result<T> Error(ErrorResult error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<T>(default, false, [error]);
    }

    public static Result<T> Error(ErrorResult[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return Error((ReadOnlySpan<ErrorResult>)errors);
    }

    public static Result<T> Error(ReadOnlySpan<ErrorResult> errors)
    {
        if (errors.IsEmpty)
            throw new ArgumentException("An error result must contain at least one error.", nameof(errors));

        foreach (var error in errors)
            ArgumentNullException.ThrowIfNull(error);

        return new Result<T>(default, false, errors.ToArray());
    }

    /// <summary>
    /// Creates an independent array of errors for APIs that require an array.
    /// </summary>
    public ErrorResult[] ErrorsToArray() => Errors.ToArray();

    internal Result<TResult> PropagateFailure<TResult>()
    {
        if (IsOk)
            throw new InvalidOperationException("Only failed results can be propagated.");

        return new Result<TResult>(default, false, _errors);
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
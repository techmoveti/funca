// Minimal local replica of Result<T> for benchmarking purposes.
// Mirrors the semantics of Funca.Abstractions.Containers.Result<T>.

namespace Funca.Benchmarks.Internal;

internal sealed class ErrorResult
{
    public string Message { get; }
    public ErrorResult(string message) => Message = message;
    public static ErrorResult Validation(string msg) => new(msg);
    public override string ToString() => Message;
}

internal readonly struct Result<T>
{
    private readonly bool _isSuccess;
    private readonly T? _value;
    private readonly ErrorResult? _singleError;
    private readonly ErrorResult[]? _manyErrors;

    private Result(T? value, bool isSuccess, ErrorResult? single, ErrorResult[]? many)
    {
        _value = value;
        _isSuccess = isSuccess;
        _singleError = single;
        _manyErrors = many;
    }

    public T? Value => _value;
    public bool IsOk => _isSuccess;
    public bool IsError => !_isSuccess;

    public T Unwrap() => _isSuccess ? _value! : throw new InvalidOperationException("Result is error.");

    public T UnwrapOr(T fallback) => _isSuccess ? _value! : fallback;

    public ErrorResult[] ErrorsToArray()
    {
        if (_manyErrors is { Length: > 0 }) return _manyErrors;
        if (_singleError is not null) return [_singleError];
        return [];
    }

    public static Result<T> Ok(T value) => new(value, true, null, null);

    public static Result<T> Error(ErrorResult error) => new(default, false, error, null);

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
        => _isSuccess ? binder(_value!) : new Result<TResult>(default, false, _singleError ?? _manyErrors?[0], null);

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
        => _isSuccess ? Result<TResult>.Ok(mapper(_value!)) : new Result<TResult>(default, false, _singleError ?? _manyErrors?[0], null);

    public Result<T> Ensure(Func<T, bool> condition, Func<ErrorResult> errorFactory)
        => _isSuccess && !condition(_value!) ? Error(errorFactory()) : this;

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ErrorResult[], TResult> onFailure)
        => _isSuccess ? onSuccess(_value!) : onFailure(ErrorsToArray());

    public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
    {
        if (!_isSuccess) return new Result<TResult>(default, false, _singleError ?? _manyErrors?[0], null);
        return Result<TResult>.Ok(await mapper(_value!).ConfigureAwait(false));
    }

    public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> binder)
    {
        if (!_isSuccess) return new Result<TResult>(default, false, _singleError ?? _manyErrors?[0], null);
        return await binder(_value!).ConfigureAwait(false);
    }

    public async ValueTask<Result<TResult>> MapValueTaskAsync<TResult>(Func<T, ValueTask<TResult>> mapper)
    {
        if (!_isSuccess) return new Result<TResult>(default, false, _singleError ?? _manyErrors?[0], null);
        return Result<TResult>.Ok(await mapper(_value!).ConfigureAwait(false));
    }

    public async ValueTask<Result<TResult>> BindValueTaskAsync<TResult>(Func<T, ValueTask<Result<TResult>>> binder)
    {
        if (!_isSuccess) return new Result<TResult>(default, false, _singleError ?? _manyErrors?[0], null);
        return await binder(_value!).ConfigureAwait(false);
    }
}

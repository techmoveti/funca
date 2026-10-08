using System.Runtime.CompilerServices;

namespace Funca.Abstractions.Containers;

[Union]
public readonly struct Result<T> : IUnion
{
    private const string NotInitialized = "The result must be initialized.";
    private const byte SuccessCase = 1;
    private const byte ErrorCase = 2;

    private readonly byte _case;
    private readonly Success<T> _success;
    private readonly ErrorCollection _errors;

    public Result(Success<T> success)
    {
        _case = SuccessCase;
        _success = success;
        _errors = default;
    }

    public Result(ErrorCollection errors)
    {
        _case = ErrorCase;
        _success = default;
        _errors = errors;
    }

    public static implicit operator Result<T>(Success<T> success) => new(success);

    public static implicit operator Result<T>(ErrorCollection errors) => new(errors);

    public bool HasValue => _case != 0;

    public bool IsOk(out Success<T> value)
    {
        value = _success;

        return _case == SuccessCase;
    }

    public bool IsError(out ErrorCollection value)
    {
        value = _errors;

        return _case == ErrorCase;
    }

    /// <summary>Returns the boxed case for compatibility. Prefer pattern matching or TryGetValue.</summary>
    public object? Value => _case switch
    {
        SuccessCase => _success,
        ErrorCase => _errors,
        _ => null
    };

    public static Result<T> Ok(T value) => new Success<T>(value);

    public static Result<T> Fail(Error error) => new ErrorCollection(error);

    public static Result<T> Fail(ErrorCollection errors) => errors;

    public static Result<T> Of(T value)
        => new Success<T>(value);

    public static Result<T> Combine(Result<T> left, Result<T> right)
    {
        return (left._case, right._case) switch
        {
            (0, _) => throw new ArgumentException(NotInitialized, nameof(left)),
            (_, 0) => throw new ArgumentException(NotInitialized, nameof(right)),
            (ErrorCase, ErrorCase)
                => Fail(left._errors.Combine(right._errors)),
            (ErrorCase, _) => left,
            (_, ErrorCase) => right,
            _ => left
        };
    }

    public T Unwrap() => _case switch
    {
        SuccessCase => _success.Value,
        ErrorCase => throw new InvalidOperationException("Cannot unwrap a failed result."),
        _ => throw new InvalidOperationException(NotInitialized)
    };

    public Result<T> Ensure(Func<T, bool> predicate)
        => Ensure(predicate, Error.Invalid("The value does not satisfy the predicate."));

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _case switch
        {
            SuccessCase => predicate(_success.Value)
                ? this
                : Fail(error),
            ErrorCase => this,
            _ => throw new InvalidOperationException(NotInitialized)
        };
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return _case switch
        {
            SuccessCase => Result<TOut>.Ok(mapper(_success.Value)),
            ErrorCase => Result<TOut>.Fail(_errors),
            _ => throw new InvalidOperationException(NotInitialized)
        };
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var result = _case switch
        {
            SuccessCase => binder(_success.Value),
            ErrorCase => Result<TOut>.Fail(_errors),
            _ => throw new InvalidOperationException(NotInitialized)
        };

        if (!result.HasValue)
            throw new InvalidOperationException("The binder returned an uninitialized result.");

        return result;
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<ErrorCollection, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return _case switch
        {
            SuccessCase => onSuccess(_success.Value),
            ErrorCase => onFailure(_errors),
            _ => throw new InvalidOperationException(NotInitialized)
        };
    }

    public Result<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        switch (_case)
        {
            case SuccessCase:
                action(_success.Value);

                break;
            case ErrorCase:
                break;
            default:
                throw new InvalidOperationException(NotInitialized);
        }

        return this;
    }

    public Result<T> Recover(Func<ErrorCollection, Result<T>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);

        var result = _case switch
        {
            SuccessCase => this,
            ErrorCase => recovery(_errors),
            _ => throw new InvalidOperationException(NotInitialized)
        };

        if (!result.HasValue)
            throw new InvalidOperationException("The recovery returned an uninitialized result.");

        return result;
    }

    public Result<T> Combine(Result<T> other) => Combine(this, other);

    public Result<TOut> Combine<TOther, TOut>(Result<TOther> other, Func<T, TOther, TOut> combiner)
    {
        ArgumentNullException.ThrowIfNull(combiner);

        return (_case, other._case) switch
        {
            (0, _) => throw new InvalidOperationException(NotInitialized),
            (_, 0) => throw new ArgumentException(NotInitialized, nameof(other)),
            (ErrorCase, ErrorCase)
                => Result<TOut>.Fail(_errors.Combine(other._errors)),
            (ErrorCase, _) => Result<TOut>.Fail(_errors),
            (_, ErrorCase) => Result<TOut>.Fail(other._errors),
            (SuccessCase, SuccessCase) => Result<TOut>.Ok(combiner(_success.Value, other._success.Value)),
            _ => throw new InvalidOperationException(NotInitialized)
        };
    }
}
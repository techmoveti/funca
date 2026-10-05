namespace Funca.Abstractions.Containers;

public union Result<T>(Success<T>, ErrorCollection)
{
    public static Result<T> Ok(T value) => new Success<T>(value);

    public static Result<T> Fail(Error error) => new ErrorCollection([error]);

    public static Result<T> Fail(ErrorCollection errors) => errors;

    public static Result<T> Of(T value)
        => new Success<T>(value);

    public Result<T> Ensure(Func<T, bool> predicate)
        => Ensure(predicate, Error.Invalid("The value does not satisfy the predicate."));

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return Value switch
        {
            Success<T> success => predicate(success.Value)
                ? this
                : Fail(error),
            ErrorCollection => this,
            _ => throw new InvalidOperationException("The result must be initialized.")
        };
    }

    public ValueTask<Result<T>> EnsureAsync(
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        CancellationToken cancellationToken = default)
        => EnsureAsync(predicate, Error.Invalid("The value does not satisfy the predicate."), cancellationToken);

    public async ValueTask<Result<T>> EnsureAsync(
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

        switch (Value)
        {
            case Success<T> success:
                var isValid = await predicate(success.Value, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                return isValid ? this : Fail(error);
            case ErrorCollection:
                return this;
            default:
                throw new InvalidOperationException("The result must be initialized.");
        }
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return Value switch
        {
            Success<T> success => Result<TOut>.Ok(mapper(success.Value)),
            ErrorCollection errors => Result<TOut>.Fail(errors),
            _ => throw new InvalidOperationException("The result must be initialized.")
        };
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var result = Value switch
        {
            Success<T> success => binder(success.Value),
            ErrorCollection errors => Result<TOut>.Fail(errors),
            _ => throw new InvalidOperationException("The result must be initialized.")
        };

        if (result.Value is null)
            throw new InvalidOperationException("The binder returned an uninitialized result.");

        return result;
    }

    public async ValueTask<Result<TOut>> BindAsync<TOut>(
        Func<T, CancellationToken, ValueTask<Result<TOut>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        switch (Value)
        {
            case Success<T> success:
                var result = await binder(success.Value, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                if (result.Value is null)
                    throw new InvalidOperationException("The binder returned an uninitialized result.");

                return result;
            case ErrorCollection errors:
                return Result<TOut>.Fail(errors);
            default:
                throw new InvalidOperationException("The result must be initialized.");
        }
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<ErrorCollection, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return Value switch
        {
            Success<T> success => onSuccess(success.Value),
            ErrorCollection errors => onFailure(errors),
            _ => throw new InvalidOperationException("The result must be initialized.")
        };
    }

    public Result<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        switch (Value)
        {
            case Success<T> success:
                action(success.Value);

                break;
            case ErrorCollection:
                break;
            default:
                throw new InvalidOperationException("The result must be initialized.");
        }

        return this;
    }

    public async ValueTask<Result<T>> TapAsync(
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        switch (Value)
        {
            case Success<T> success:
                await action(success.Value, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                break;
            case ErrorCollection:
                break;
            default:
                throw new InvalidOperationException("The result must be initialized.");
        }

        return this;
    }

    public Result<T> Recover(Func<ErrorCollection, Result<T>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);

        var result = Value switch
        {
            Success<T> => this,
            ErrorCollection errors => recovery(errors),
            _ => throw new InvalidOperationException("The result must be initialized.")
        };

        if (result.Value is null)
            throw new InvalidOperationException("The recovery returned an uninitialized result.");

        return result;
    }

    public Result<T> Combine(Result<T> other) => Combine(this, other);

    public Result<TOut> Combine<TOther, TOut>(Result<TOther> other, Func<T, TOther, TOut> combiner)
    {
        ArgumentNullException.ThrowIfNull(combiner);

        return (Value, other.Value) switch
        {
            (null, _) => throw new InvalidOperationException("The result must be initialized."),
            (_, null) => throw new ArgumentException("The result must be initialized.", nameof(other)),
            (ErrorCollection leftErrors, ErrorCollection rightErrors)
                => Result<TOut>.Fail(new ErrorCollection([.. leftErrors.Errors, .. rightErrors.Errors])),
            (ErrorCollection errors, _) => Result<TOut>.Fail(errors),
            (_, ErrorCollection errors) => Result<TOut>.Fail(errors),
            (Success<T> left, Success<TOther> right) => Result<TOut>.Ok(combiner(left.Value, right.Value)),
            _ => throw new InvalidOperationException("The result must be initialized.")
        };
    }

    public static Result<T> Combine(Result<T> left, Result<T> right)
    {
        return (left.Value, right.Value) switch
        {
            (null, _) => throw new ArgumentException("The result must be initialized.", nameof(left)),
            (_, null) => throw new ArgumentException("The result must be initialized.", nameof(right)),
            (ErrorCollection leftErrors, ErrorCollection rightErrors)
                => Fail(new ErrorCollection([.. leftErrors.Errors, .. rightErrors.Errors])),
            (ErrorCollection, _) => left,
            (_, ErrorCollection) => right,
            _ => left
        };
    }
}
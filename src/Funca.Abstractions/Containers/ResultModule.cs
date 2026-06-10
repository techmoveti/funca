namespace Funca.Abstractions.Containers;

public static partial class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Wrap(value);

    public static Result<T> Error<T>(string errorMessage)
        => Result<T>.Error(
            ErrorResult.Validation(errorMessage ?? throw new ArgumentNullException(nameof(errorMessage))));

    public static Result<T> Error<T>(ErrorResult error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return Result<T>.Error(error);
    }

    public static Result<T> Error<T>(ErrorResult[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return Result<T>.Error(errors);
    }

    public static async Task<TResult> Match<T, TResult>(
        this Task<Result<T>> @this,
        Func<T, Task<TResult>> onSuccess,
        Func<ErrorResult[], Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await @this;

        return result.IsOk
            ? await onSuccess(result.Unwrap())
            : await onFailure(result.Errors);
    }

    extension<T>(Result<T> @this)
    {
        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsOk
                ? binder(@this.Value!)
                : Error<TResult>(@this.Errors);
        }

        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return @this.IsOk
                ? Ok(mapper(@this.Value!))
                : Error<TResult>(@this.Errors);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ErrorResult[], TResult> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            return @this.IsOk
                ? onSuccess(@this.Unwrap())
                : onFailure(@this.Errors);
        }
    }

    extension<T>(Result<T> @this)
    {
        public Result<T> Filter(Func<T, bool> predicate, Func<T, ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(errorFactory);

            return @this.IsError
                ? @this
                : predicate(@this.Value!)
                    ? @this
                    : Error<T>(errorFactory(@this.Value!));
        }

        public Result<T> Ensure(Func<T, bool> condition, Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            return @this.IsError
                ? @this
                : condition(@this.Value!)
                    ? @this
                    : Error<T>(errorFactory());
        }
    }
}
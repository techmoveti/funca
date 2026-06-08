using static Funca.Abstractions.Containers.Result;

namespace Funca.Abstractions.Containers;

public static partial class Option
{
    public static Option<T> Some<T>(T value) => value is null
        ? throw new ArgumentNullException(nameof(value))
        : Option<T>.Some(value);

    public static Option<T> None<T>() => Option<T>.None();

    public static async Task<Result<T>> ToResult<T>(this Task<Option<T>> @this, ErrorResult error)
    {
        var option = await @this;

        return option.IsNone
            ? Error<T>(error)
            : Ok(option.Value!);
    }

    extension<T>(Option<T> @this)
    {
        public Result<T> ToResult() => @this.ToResult(ErrorResult.Empty);

        public Result<T> ToResult(ErrorResult error) => @this.IsNone
            ? Error<T>(error)
            : Ok(@this.Value!);

        public Result<T> ToResult(Func<T, Result<T>> onSome, Func<Result<T>> onNone) => @this.IsSome
            ? onSome(@this.Value!)
            : onNone();
    }

    extension<T>(Option<T> @this)
    {
        public Option<T> Bind(Func<T, Option<T>> onSome) => @this.IsSome
            ? onSome(@this.Value!)
            : @this;

        public Option<T> Ensure(Func<T, bool> condition) => @this.IsSome
            ? condition(@this.Value!)
                ? @this
                : None<T>()
            : None<T>();
    }
}
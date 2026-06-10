using static Funca.Abstractions.Containers.Result;

namespace Funca.Abstractions.Containers;

public static partial class Option
{
    public static Option<T> Some<T>(T value) => value is null
        ? throw new ArgumentNullException(nameof(value))
        : Option<T>.Some(value);

    public static Option<T> None<T>() => Option<T>.None();

    extension<T>(Option<T> @this)
    {
        // =========================
        // Result
        // =========================

        public Result<T> ToResult() =>
            @this.ToResult(ErrorResult.Empty);

        public Result<T> ToResult(ErrorResult error) =>
            @this.IsNone
                ? Error<T>(error)
                : Ok(@this.Value!);

        public Result<T> ToResult(
            Func<T, Result<T>> onSome,
            Func<Result<T>> onNone) =>
            @this.IsSome
                ? onSome(@this.Value!)
                : onNone();

        // =========================
        // Bind
        // =========================

        public Option<T> Bind(Func<T, Option<T>> binder) =>
            @this.IsSome
                ? binder(@this.Value!)
                : @this;

        // =========================
        // Map
        // =========================

        public Option<TResult> Map<TResult>(
            Func<T, TResult> mapper) =>
            @this.IsSome
                ? Some(mapper(@this.Value!))
                : None<TResult>();

        // =========================
        // Ensure
        // =========================

        public Option<T> Ensure(
            Func<T, bool> predicate) =>
            @this.IsSome
                ? predicate(@this.Value!)
                    ? @this
                    : None<T>()
                : None<T>();

        // =========================
        // Match
        // =========================

        public TResult Match<TResult>(
            Func<T, TResult> onSome,
            Func<TResult> onNone) =>
            @this.IsSome
                ? onSome(@this.Value!)
                : onNone();

        // =========================
        // Fallback
        // =========================

        public Option<T> OrElse(
            Func<Option<T>> fallback) =>
            @this.IsSome
                ? @this
                : fallback();

        public Option<T> OrElse(
            T fallbackValue) =>
            @this.IsSome
                ? @this
                : Some(fallbackValue);
    }
}
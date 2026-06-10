using static Funca.Abstractions.Containers.Result;

namespace Funca.Abstractions.Containers;

public static partial class Option
{
    extension<T>(Option<T> @this)
    {
        // =========================
        // ToResult
        // =========================

        public async Task<Result<T>> ToResult(
            Func<T, Task<Result<T>>> onSome,
            Func<Task<Result<T>>> onNone) =>
            @this.IsSome
                ? await onSome(@this.Value!)
                : await onNone();

        public async ValueTask<Result<T>> ToResult(
            Func<T, ValueTask<Result<T>>> onSome,
            Func<ValueTask<Result<T>>> onNone) =>
            @this.IsSome
                ? await onSome(@this.Value!)
                : await onNone();

        // =========================
        // Bind
        // =========================

        public async Task<Option<T>> Bind(
            Func<T, Task<Option<T>>> binder) =>
            @this.IsSome
                ? await binder(@this.Value!)
                : @this;

        public async ValueTask<Option<T>> Bind(
            Func<T, ValueTask<Option<T>>> binder) =>
            @this.IsSome
                ? await binder(@this.Value!)
                : @this;

        // =========================
        // Map
        // =========================

        public async Task<Option<TResult>> Map<TResult>(
            Func<T, Task<TResult>> mapper) =>
            @this.IsSome
                ? Some(await mapper(@this.Value!))
                : None<TResult>();

        public async ValueTask<Option<TResult>> Map<TResult>(
            Func<T, ValueTask<TResult>> mapper) =>
            @this.IsSome
                ? Some(await mapper(@this.Value!))
                : None<TResult>();

        // =========================
        // Ensure
        // =========================

        public async Task<Option<T>> Ensure(
            Func<T, Task<bool>> predicate) =>
            @this.IsSome
                ? await predicate(@this.Value!)
                    ? @this
                    : None<T>()
                : None<T>();

        public async ValueTask<Option<T>> Ensure(
            Func<T, ValueTask<bool>> predicate) =>
            @this.IsSome
                ? await predicate(@this.Value!)
                    ? @this
                    : None<T>()
                : None<T>();

        // =========================
        // Match
        // =========================

        public async Task<TResult> Match<TResult>(
            Func<T, Task<TResult>> onSome,
            Func<Task<TResult>> onNone) =>
            @this.IsSome
                ? await onSome(@this.Value!)
                : await onNone();

        public async ValueTask<TResult> Match<TResult>(
            Func<T, ValueTask<TResult>> onSome,
            Func<ValueTask<TResult>> onNone) =>
            @this.IsSome
                ? await onSome(@this.Value!)
                : await onNone();

        // =========================
        // OrElse
        // =========================

        public async Task<Option<T>> OrElse(
            Func<Task<Option<T>>> fallback) =>
            @this.IsSome
                ? @this
                : await fallback();

        public async ValueTask<Option<T>> OrElse(
            Func<ValueTask<Option<T>>> fallback) =>
            @this.IsSome
                ? @this
                : await fallback();
    }
}
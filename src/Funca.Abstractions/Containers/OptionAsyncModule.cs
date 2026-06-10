using static Funca.Abstractions.Containers.Result;

namespace Funca.Abstractions.Containers;

public static partial class Option
{
    extension<T>(Option<T> @this)
    {
        // =========================
        // ToResult
        // =========================

        public Task<Result<T>> ToResult(
            Func<T, Task<Result<T>>> onSome,
            Func<Task<Result<T>>> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            return @this.IsSome
                ? onSome(@this.Value!)
                : onNone();
        }

        public ValueTask<Result<T>> ToResult(
            Func<T, ValueTask<Result<T>>> onSome,
            Func<ValueTask<Result<T>>> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            return @this.IsSome
                ? onSome(@this.Value!)
                : onNone();
        }

        // =========================
        // Bind
        // =========================

        public Task<Option<TResult>> Bind<TResult>(
            Func<T, Task<Option<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsSome
                ? binder(@this.Value!)
                : Task.FromResult(None<TResult>());
        }

        public ValueTask<Option<TResult>> Bind<TResult>(
            Func<T, ValueTask<Option<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsSome
                ? binder(@this.Value!)
                : new ValueTask<Option<TResult>>(None<TResult>());
        }

        // =========================
        // Map
        // =========================

        public async Task<Option<TResult>> Map<TResult>(
            Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return @this.IsSome
                ? From(await mapper(@this.Value!))
                : None<TResult>();
        }

        public async ValueTask<Option<TResult>> Map<TResult>(
            Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return @this.IsSome
                ? From(await mapper(@this.Value!))
                : None<TResult>();
        }

        // =========================
        // Ensure
        // =========================

        public async Task<Option<T>> Ensure(
            Func<T, Task<bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return @this.IsSome
                ? await predicate(@this.Value!)
                    ? @this
                    : None<T>()
                : None<T>();
        }

        public async ValueTask<Option<T>> Ensure(
            Func<T, ValueTask<bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return @this.IsSome
                ? await predicate(@this.Value!)
                    ? @this
                    : None<T>()
                : None<T>();
        }

        // =========================
        // Match
        // =========================

        public Task<TResult> Match<TResult>(
            Func<T, Task<TResult>> onSome,
            Func<Task<TResult>> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            return @this.IsSome
                ? onSome(@this.Value!)
                : onNone();
        }

        public ValueTask<TResult> Match<TResult>(
            Func<T, ValueTask<TResult>> onSome,
            Func<ValueTask<TResult>> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            return @this.IsSome
                ? onSome(@this.Value!)
                : onNone();
        }

        // =========================
        // OrElse
        // =========================

        public Task<Option<T>> OrElse(
            Func<Task<Option<T>>> fallback)
        {
            ArgumentNullException.ThrowIfNull(fallback);

            return @this.IsSome
                ? Task.FromResult(@this)
                : fallback();
        }

        public ValueTask<Option<T>> OrElse(
            Func<ValueTask<Option<T>>> fallback)
        {
            ArgumentNullException.ThrowIfNull(fallback);

            return @this.IsSome
                ? new ValueTask<Option<T>>(@this)
                : fallback();
        }
    }
}
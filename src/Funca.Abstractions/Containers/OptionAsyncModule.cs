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

        public ValueTask<Result<T>> ToResultValueTask(
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

        public ValueTask<Option<TResult>> BindValueTask<TResult>(
            Func<T, ValueTask<Option<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsSome
                ? binder(@this.Value!)
                : ValueTask.FromResult(None<TResult>());
        }

        // =========================
        // Map
        // =========================

        public Task<Option<TResult>> Map<TResult>(
            Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@this.IsNone)
                return Task.FromResult(None<TResult>());

            return ExecuteAsync(@this.Value!, mapper);

            static async Task<Option<TResult>> ExecuteAsync(
                T value,
                Func<T, Task<TResult>> mapper)
            {
                return From(await mapper(value));
            }
        }

        public ValueTask<Option<TResult>> MapValueTask<TResult>(
            Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@this.IsNone)
                return ValueTask.FromResult(None<TResult>());

            return ExecuteAsync(@this.Value!, mapper);

            static async ValueTask<Option<TResult>> ExecuteAsync(
                T value,
                Func<T, ValueTask<TResult>> mapper)
            {
                return From(await mapper(value));
            }
        }

        // =========================
        // Ensure
        // =========================

        public Task<Option<T>> Ensure(
            Func<T, Task<bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            if (@this.IsNone)
                return Task.FromResult(None<T>());

            return ExecuteAsync(@this, predicate);

            static async Task<Option<T>> ExecuteAsync(
                Option<T> option,
                Func<T, Task<bool>> predicate)
            {
                return await predicate(option.Value!)
                    ? option
                    : None<T>();
            }
        }

        public ValueTask<Option<T>> EnsureValueTask(
            Func<T, ValueTask<bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            if (@this.IsNone)
                return ValueTask.FromResult(None<T>());

            return ExecuteAsync(@this, predicate);

            static async ValueTask<Option<T>> ExecuteAsync(
                Option<T> option,
                Func<T, ValueTask<bool>> predicate)
            {
                return await predicate(option.Value!)
                    ? option
                    : None<T>();
            }
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

        public ValueTask<TResult> MatchValueTask<TResult>(
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

        public ValueTask<Option<T>> OrElseValueTask(
            Func<ValueTask<Option<T>>> fallback)
        {
            ArgumentNullException.ThrowIfNull(fallback);

            return @this.IsSome
                ? ValueTask.FromResult(@this)
                : fallback();
        }
    }
}
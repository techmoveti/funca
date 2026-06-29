namespace Funca.Abstractions.Containers;

public static partial class Result
{
    extension<T>(Result<T> @this)
    {
        // =========================
        // Bind
        // =========================

        public Task<Result<TResult>> Bind<TResult>(Func<T, Task<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsOk
                ? binder(@this.Value!)
                : Task.FromResult(Error<TResult>(@this.Errors));
        }

        public ValueTask<Result<TResult>> BindValueTask<TResult>(
            Func<T, ValueTask<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsOk
                ? binder(@this.Value!)
                : ValueTask.FromResult(Error<TResult>(@this.Errors));
        }

        // =========================
        // Map
        // =========================

        public Task<Result<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@this.IsError)
                return Task.FromResult(Error<TResult>(@this.Errors));

            return ExecuteAsync(@this.Value!, mapper);

            static async Task<Result<TResult>> ExecuteAsync(
                T value,
                Func<T, Task<TResult>> mapper) => Ok(await mapper(value));
        }

        public ValueTask<Result<TResult>> MapValueTask<TResult>(
            Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@this.IsError)
                return ValueTask.FromResult(Error<TResult>(@this.Errors));

            return ExecuteAsync(@this.Value!, mapper);

            static async ValueTask<Result<TResult>> ExecuteAsync(
                T value,
                Func<T, ValueTask<TResult>> mapper) => Ok(await mapper(value));
        }

        // =========================
        // Ensure
        // =========================

        public Task<Result<T>> Ensure(
            Func<T, Task<bool>> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            if (@this.IsError)
                return Task.FromResult(@this);

            return ExecuteAsync(@this, condition, errorFactory);

            static async Task<Result<T>> ExecuteAsync(
                Result<T> result,
                Func<T, Task<bool>> condition,
                Func<ErrorResult> errorFactory) => await condition(result.Value!)
                ? result
                : Error<T>(errorFactory());
        }

        public ValueTask<Result<T>> EnsureValueTask(
            Func<T, ValueTask<bool>> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            if (@this.IsError)
                return ValueTask.FromResult(@this);

            return ExecuteAsync(@this, condition, errorFactory);

            static async ValueTask<Result<T>> ExecuteAsync(
                Result<T> result,
                Func<T, ValueTask<bool>> condition,
                Func<ErrorResult> errorFactory) => await condition(result.Value!)
                ? result
                : Error<T>(errorFactory());
        }
    }

    extension<T>(Task<Result<T>> @this)
    {
        // =========================
        // Bind
        // =========================

        public async Task<Result<TResult>> Bind<TResult>(
            Func<T, Result<TResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async Task<Result<TResult>> Bind<TResult>(
            Func<T, Task<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> BindValueTask<TResult>(
            Func<T, ValueTask<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        // =========================
        // Map
        // =========================

        public async Task<Result<TResult>> Map<TResult>(
            Func<T, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async Task<Result<TResult>> Map<TResult>(
            Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> MapValueTask<TResult>(
            Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        // =========================
        // Ensure
        // =========================

        public async Task<Result<T>> Ensure(
            Func<T, bool> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            var result = await @this;

            return result.IsError
                ? result
                : condition(result.Value!)
                    ? result
                    : Error<T>(errorFactory());
        }

        public async Task<Result<T>> Ensure(
            Func<T, Task<bool>> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            var result = await @this;

            return result.IsError
                ? result
                : await condition(result.Value!)
                    ? result
                    : Error<T>(errorFactory());
        }

        public async ValueTask<Result<T>> EnsureValueTask(
            Func<T, ValueTask<bool>> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            var result = await @this;

            return result.IsError
                ? result
                : await condition(result.Value!)
                    ? result
                    : Error<T>(errorFactory());
        }
    }

    extension<T>(ValueTask<Result<T>> @this)
    {
        // =========================
        // Bind
        // =========================

        public async ValueTask<Result<TResult>> Bind<TResult>(
            Func<T, Result<TResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Bind<TResult>(
            Func<T, Task<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> BindValueTask<TResult>(
            Func<T, ValueTask<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        // =========================
        // Map
        // =========================

        public async ValueTask<Result<TResult>> Map<TResult>(
            Func<T, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Map<TResult>(
            Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> MapValueTask<TResult>(
            Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        // =========================
        // Ensure
        // =========================

        public async ValueTask<Result<T>> Ensure(
            Func<T, bool> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            var result = await @this;

            return result.IsError
                ? result
                : condition(result.Value!)
                    ? result
                    : Error<T>(errorFactory());
        }

        public async ValueTask<Result<T>> Ensure(
            Func<T, Task<bool>> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            var result = await @this;

            return result.IsError
                ? result
                : await condition(result.Value!)
                    ? result
                    : Error<T>(errorFactory());
        }

        public async ValueTask<Result<T>> EnsureValueTask(
            Func<T, ValueTask<bool>> condition,
            Func<ErrorResult> errorFactory)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(errorFactory);

            var result = await @this;

            return result.IsError
                ? result
                : await condition(result.Value!)
                    ? result
                    : Error<T>(errorFactory());
        }
    }
}

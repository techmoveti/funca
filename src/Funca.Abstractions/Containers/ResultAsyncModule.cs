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

        public ValueTask<Result<TResult>> Bind<TResult>(Func<T, ValueTask<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return @this.IsOk
                ? binder(@this.Value!)
                : new ValueTask<Result<TResult>>(Error<TResult>(@this.Errors));
        }

        // =========================
        // Map
        // =========================

        public async Task<Result<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return @this.IsOk
                ? Ok(await mapper(@this.Value!))
                : Error<TResult>(@this.Errors);
        }

        public async ValueTask<Result<TResult>> Map<TResult>(Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return @this.IsOk
                ? Ok(await mapper(@this.Value!))
                : Error<TResult>(@this.Errors);
        }
    }

    extension<T>(Task<Result<T>> @this)
    {
        // =========================
        // Bind
        // =========================

        public async Task<Result<TResult>> Bind<TResult>(Func<T, Task<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async Task<Result<TResult>> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Bind<TResult>(Func<T, ValueTask<Result<TResult>>> binder)
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

        public async Task<Result<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async Task<Result<TResult>> Map<TResult>(Func<T, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Map<TResult>(Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }
    }

    extension<T>(ValueTask<Result<T>> @this)
    {
        // =========================
        // Bind
        // =========================

        public async ValueTask<Result<TResult>> Bind<TResult>(Func<T, ValueTask<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Bind<TResult>(Func<T, Task<Result<TResult>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            var result = await @this;

            return result.IsOk
                ? binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        // =========================
        // Map
        // =========================

        public async ValueTask<Result<TResult>> Map<TResult>(Func<T, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async ValueTask<Result<TResult>> Map<TResult>(Func<T, ValueTask<TResult>> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }
    }
}
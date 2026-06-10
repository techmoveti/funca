namespace Funca.Abstractions.Containers;

public static partial class Result
{
    extension<T>(Result<T> @this)
    {
        // =========================
        // Tee
        // =========================

        public Result<T> Tee(Action<T> action)
        {
            if (@this.IsError)
                return @this;

            action(@this.Value!);

            return @this;
        }

        public async Task<Result<T>> Tee(Func<T, Task> action)
        {
            if (@this.IsError)
                return @this;

            await action(@this.Value!);

            return @this;
        }

        public async ValueTask<Result<T>> Tee(Func<T, ValueTask> action)
        {
            if (@this.IsError)
                return @this;

            await action(@this.Value!);

            return @this;
        }

        // =========================
        // Adaptadores
        // =========================

        public Task<Result<T>> TeeFromResult(Action<T> action)
        {
            if (@this.IsError)
                return Task.FromResult(@this);

            action(@this.Value!);

            return Task.FromResult(@this);
        }

        public ValueTask<Result<T>> TeeFromValueTask(Action<T> action)
        {
            if (@this.IsError)
                return ValueTask.FromResult(@this);

            action(@this.Value!);

            return ValueTask.FromResult(@this);
        }
    }

    extension<T>(Task<Result<T>> @this)
    {
        // =========================
        // Tee Sync
        // =========================

        public async Task<Result<T>> Tee(Action<T> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            action(result.Value!);

            return result;
        }

        // =========================
        // Tee Task
        // =========================

        public async Task<Result<T>> Tee(Func<T, Task> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            await action(result.Value!);

            return result;
        }

        // =========================
        // Tee ValueTask
        // =========================

        public async ValueTask<Result<T>> Tee(Func<T, ValueTask> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            await action(result.Value!);

            return result;
        }
    }

    extension<T>(ValueTask<Result<T>> @this)
    {
        // =========================
        // Tee Sync
        // =========================

        public async ValueTask<Result<T>> Tee(Action<T> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            action(result.Value!);

            return result;
        }

        // =========================
        // Tee Task
        // =========================

        public async ValueTask<Result<T>> Tee(Func<T, Task> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            await action(result.Value!);

            return result;
        }

        // =========================
        // Tee ValueTask
        // =========================

        public async ValueTask<Result<T>> Tee(Func<T, ValueTask> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            await action(result.Value!);

            return result;
        }
    }

    extension<TValue>(Result<TValue> @this)
    {
        // =========================
        // Match
        // =========================

        public void Match(
            Action<TValue> onSuccess,
            Action<ErrorResult[]> onFailure)
        {
            if (@this.IsOk)
                onSuccess(@this.Unwrap());
            else
                onFailure(@this.Errors);
        }

        public async Task Match(
            Func<TValue, Task> onSuccess,
            Func<ErrorResult[], Task> onFailure)
        {
            if (@this.IsOk)
                await onSuccess(@this.Unwrap());
            else
                await onFailure(@this.Errors);
        }

        public async ValueTask Match(
            Func<TValue, ValueTask> onSuccess,
            Func<ErrorResult[], ValueTask> onFailure)
        {
            if (@this.IsOk)
                await onSuccess(@this.Unwrap());
            else
                await onFailure(@this.Errors);
        }
    }

    extension<TValue>(Task<Result<TValue>> @this)
    {
        public async Task Match(
            Action<TValue> onSuccess,
            Action<ErrorResult[]> onFailure)
        {
            var result = await @this;

            if (result.IsOk)
                onSuccess(result.Unwrap());
            else
                onFailure(result.Errors);
        }

        public async Task Match(
            Func<TValue, Task> onSuccess,
            Func<ErrorResult[], Task> onFailure)
        {
            var result = await @this;

            if (result.IsOk)
                await onSuccess(result.Unwrap());
            else
                await onFailure(result.Errors);
        }

        public async ValueTask Match(
            Func<TValue, ValueTask> onSuccess,
            Func<ErrorResult[], ValueTask> onFailure)
        {
            var result = await @this;

            if (result.IsOk)
                await onSuccess(result.Unwrap());
            else
                await onFailure(result.Errors);
        }
    }

    extension<TValue>(ValueTask<Result<TValue>> @this)
    {
        public async ValueTask Match(
            Action<TValue> onSuccess,
            Action<ErrorResult[]> onFailure)
        {
            var result = await @this;

            if (result.IsOk)
                onSuccess(result.Unwrap());
            else
                onFailure(result.Errors);
        }

        public async ValueTask Match(
            Func<TValue, Task> onSuccess,
            Func<ErrorResult[], Task> onFailure)
        {
            var result = await @this;

            if (result.IsOk)
                await onSuccess(result.Unwrap());
            else
                await onFailure(result.Errors);
        }

        public async ValueTask Match(
            Func<TValue, ValueTask> onSuccess,
            Func<ErrorResult[], ValueTask> onFailure)
        {
            var result = await @this;

            if (result.IsOk)
                await onSuccess(result.Unwrap());
            else
                await onFailure(result.Errors);
        }
    }
}
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
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return @this;

            action(@this.Value!);

            return @this;
        }

        public Task<Result<T>> Tee(Func<T, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return Task.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async Task<Result<T>> ExecuteAsync(
                Result<T> result,
                Func<T, Task> action)
            {
                await action(result.Value!).ConfigureAwait(false);

                return result;
            }
        }

        public ValueTask<Result<T>> TeeValueTask(Func<T, ValueTask> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return ValueTask.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async ValueTask<Result<T>> ExecuteAsync(
                Result<T> result,
                Func<T, ValueTask> action)
            {
                await action(result.Value!).ConfigureAwait(false);

                return result;
            }
        }

        // =========================
        // Adaptadores
        // =========================

        public Task<Result<T>> TeeAsTask(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return Task.FromResult(@this);

            action(@this.Value!);

            return Task.FromResult(@this);
        }

        public ValueTask<Result<T>> TeeAsValueTask(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return ValueTask.FromResult(@this);

            action(@this.Value!);

            return ValueTask.FromResult(@this);
        }

        public Task<Result<T>> TeeFromTask(Func<T, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return Task.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async Task<Result<T>> ExecuteAsync(
                Result<T> result,
                Func<T, Task> action)
            {
                await action(result.Value!).ConfigureAwait(false);

                return result;
            }
        }

        public ValueTask<Result<T>> TeeFromValueTask(Func<T, ValueTask> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsError)
                return ValueTask.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async ValueTask<Result<T>> ExecuteAsync(
                Result<T> result,
                Func<T, ValueTask> action)
            {
                await action(result.Value!).ConfigureAwait(false);

                return result;
            }
        }
    }

    extension<T>(Task<Result<T>> @this)
    {
        // =========================
        // Tee Sync
        // =========================

        public async Task<Result<T>> Tee(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(action);

            var result = await @this.ConfigureAwait(false);

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
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(action);

            var result = await @this.ConfigureAwait(false);

            if (result.IsError)
                return result;

            await action(result.Value!).ConfigureAwait(false);

            return result;
        }

        // =========================
        // Tee ValueTask
        // =========================

        public async ValueTask<Result<T>> TeeValueTask(Func<T, ValueTask> action)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(action);

            var result = await @this.ConfigureAwait(false);

            if (result.IsError)
                return result;

            await action(result.Value!).ConfigureAwait(false);

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
            ArgumentNullException.ThrowIfNull(action);

            var result = await @this.ConfigureAwait(false);

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
            ArgumentNullException.ThrowIfNull(action);

            var result = await @this.ConfigureAwait(false);

            if (result.IsError)
                return result;

            await action(result.Value!).ConfigureAwait(false);

            return result;
        }

        // =========================
        // Tee ValueTask
        // =========================

        public async ValueTask<Result<T>> TeeValueTask(Func<T, ValueTask> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var result = await @this.ConfigureAwait(false);

            if (result.IsError)
                return result;

            await action(result.Value!).ConfigureAwait(false);

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
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (@this.IsOk)
                onSuccess(@this.Unwrap());
            else
                onFailure(@this.ErrorsToArray());
        }

        public async Task Match(
            Func<TValue, Task> onSuccess,
            Func<ErrorResult[], Task> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (@this.IsOk)
                await onSuccess(@this.Unwrap()).ConfigureAwait(false);
            else
                await onFailure(@this.ErrorsToArray()).ConfigureAwait(false);
        }

        public async ValueTask MatchValueTask(
            Func<TValue, ValueTask> onSuccess,
            Func<ErrorResult[], ValueTask> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (@this.IsOk)
                await onSuccess(@this.Unwrap()).ConfigureAwait(false);
            else
                await onFailure(@this.ErrorsToArray()).ConfigureAwait(false);
        }
    }

    extension<TValue>(Task<Result<TValue>> @this)
    {
        public async Task Match(
            Action<TValue> onSuccess,
            Action<ErrorResult[]> onFailure)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            var result = await @this.ConfigureAwait(false);

            if (result.IsOk)
                onSuccess(result.Unwrap());
            else
                onFailure(result.ErrorsToArray());
        }

        public async Task Match(
            Func<TValue, Task> onSuccess,
            Func<ErrorResult[], Task> onFailure)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            var result = await @this.ConfigureAwait(false);

            if (result.IsOk)
                await onSuccess(result.Unwrap()).ConfigureAwait(false);
            else
                await onFailure(result.ErrorsToArray()).ConfigureAwait(false);
        }

        public async ValueTask MatchValueTask(
            Func<TValue, ValueTask> onSuccess,
            Func<ErrorResult[], ValueTask> onFailure)
        {
            ArgumentNullException.ThrowIfNull(@this);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            var result = await @this.ConfigureAwait(false);

            if (result.IsOk)
                await onSuccess(result.Unwrap()).ConfigureAwait(false);
            else
                await onFailure(result.ErrorsToArray()).ConfigureAwait(false);
        }
    }

    extension<TValue>(ValueTask<Result<TValue>> @this)
    {
        public async ValueTask Match(
            Action<TValue> onSuccess,
            Action<ErrorResult[]> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            var result = await @this.ConfigureAwait(false);

            if (result.IsOk)
                onSuccess(result.Unwrap());
            else
                onFailure(result.ErrorsToArray());
        }

        public async ValueTask Match(
            Func<TValue, Task> onSuccess,
            Func<ErrorResult[], Task> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            var result = await @this.ConfigureAwait(false);

            if (result.IsOk)
                await onSuccess(result.Unwrap()).ConfigureAwait(false);
            else
                await onFailure(result.ErrorsToArray()).ConfigureAwait(false);
        }

        public async ValueTask MatchValueTask(
            Func<TValue, ValueTask> onSuccess,
            Func<ErrorResult[], ValueTask> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            var result = await @this.ConfigureAwait(false);

            if (result.IsOk)
                await onSuccess(result.Unwrap()).ConfigureAwait(false);
            else
                await onFailure(result.ErrorsToArray()).ConfigureAwait(false);
        }
    }
}
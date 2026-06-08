namespace Funca.Abstractions.Containers;

public static partial class Result
{
    extension<T>(Result<T> @this)
    {
        public Result<T> Tee(Action<T> action)
        {
            if (@this.IsError)
                return @this;

            action(@this.Value!);

            return @this;
        }

        public Task<Result<T>> TeeFromResult(Action<T> action)
        {
            if (@this.IsError)
                return Task.FromResult(@this);

            action(@this.Value!);

            return Task.FromResult(@this);
        }
    }

    extension<T>(Task<Result<T>> @this)
    {
        public async Task<Result<T>> Tee(Action<T> action)
        {
            var result = await @this;

            if (result.IsError)
                return result;

            action(result.Value!);

            return result;
        }

        public async Task<Result<T>> Tee(Func<T, Task<Action<T>>> action)
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
        public void Match(Action<TValue> onSuccess, Action<ErrorResult[]> onFailure)
        {
            if (@this.IsOk)
                onSuccess(@this.Unwrap());
            else
                onFailure(@this.Errors);
        }
    }
}
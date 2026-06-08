namespace Funca.Abstractions.Containers;

public static partial class Result
{
    extension<T>(Result<T> @this)
    {
        public async Task<Result<TResult>> Bind<TResult>(Func<T, Task<Result<TResult>>> binder) => @this.IsOk
            ? await binder(@this.Value!)
            : Error<TResult>(@this.Errors);

        public async Task<Result<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper) => @this.IsOk
            ? Ok(await mapper(@this.Value!))
            : Error<TResult>(@this.Errors);
    }

    extension<T>(Task<Result<T>> @this)
    {
        public async Task<Result<TResult>> Bind<TResult>(Func<T, Task<Result<TResult>>> binder)
        {
            var result = await @this;

            return result.IsOk
                ? await binder(result.Value!)
                : Error<TResult>(result.Errors);
        }

        public async Task<Result<TResult>> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            var result = await @this;

            return result.IsOk
                ? binder(result.Value!)
                : Error<TResult>(result.Errors);
        }
    }

    extension<T>(Task<Result<T>> @this)
    {
        public async Task<Result<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper)
        {
            var result = await @this;

            return result.IsOk
                ? Ok(await mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }

        public async Task<Result<TResult>> Map<TResult>(Func<T, TResult> mapper)
        {
            var result = await @this;

            return result.IsOk
                ? Ok(mapper(result.Value!))
                : Error<TResult>(result.Errors);
        }
    }
}
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Funca.Benchmarks.Internal;

namespace Funca.Benchmarks;

/// <summary>
/// Benchmarks for async Task and ValueTask Result pipelines.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class ResultAsyncBenchmarks
{
    private static readonly ErrorResult _validationError = ErrorResult.Validation("invalid");

    // ── Task chain (success) ───────────────────────────────────────────────

    [Benchmark]
    public Task<string> TaskChain_Success()
    {
        return Result<int>.Ok(1)
            .MapAsync(x => Task.FromResult(x + 1))
            .ContinueWithResult(r => r.BindAsync(x => Task.FromResult(Result<int>.Ok(x * 2))))
            .ContinueWithResult(r => Task.FromResult(r.Match(x => $"ok:{x}", _ => "fail")));
    }

    // ── ValueTask chain (success) ──────────────────────────────────────────

    [Benchmark]
    public ValueTask<string> ValueTaskChain_Success()
    {
        return Result<int>.Ok(1)
            .MapValueTaskAsync(x => ValueTask.FromResult(x + 1))
            .ContinueWithResultValueTask(r => r.BindValueTaskAsync(x => ValueTask.FromResult(Result<int>.Ok(x * 2))))
            .ContinueWithResultValueTask(r => ValueTask.FromResult(r.Match(x => $"ok:{x}", _ => "fail")));
    }

    // ── Task chain (failure) ───────────────────────────────────────────────

    [Benchmark]
    public Task<string> TaskChain_EarlyFailure()
    {
        return Result<int>.Error(_validationError)
            .MapAsync(x => Task.FromResult(x + 1))       // skipped
            .ContinueWithResult(r => Task.FromResult(r.Match(x => $"ok:{x}", _ => "fail")));
    }
}

// Helper extensions to chain async Results in the benchmark (not part of the library API).
internal static class TaskResultExtensions
{
    public static async Task<Result<TResult>> ContinueWithResult<T, TResult>(
        this Task<Result<T>> source,
        Func<Result<T>, Task<Result<TResult>>> next)
    {
        var r = await source.ConfigureAwait(false);
        return await next(r).ConfigureAwait(false);
    }

    public static async Task<TResult> ContinueWithResult<T, TResult>(
        this Task<Result<T>> source,
        Func<Result<T>, Task<TResult>> next)
    {
        var r = await source.ConfigureAwait(false);
        return await next(r).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TResult>> ContinueWithResultValueTask<T, TResult>(
        this ValueTask<Result<T>> source,
        Func<Result<T>, ValueTask<Result<TResult>>> next)
    {
        var r = await source.ConfigureAwait(false);
        return await next(r).ConfigureAwait(false);
    }

    public static async ValueTask<TResult> ContinueWithResultValueTask<T, TResult>(
        this ValueTask<Result<T>> source,
        Func<Result<T>, ValueTask<TResult>> next)
    {
        var r = await source.ConfigureAwait(false);
        return await next(r).ConfigureAwait(false);
    }
}

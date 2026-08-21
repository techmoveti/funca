using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Funca.Benchmarks.Internal;

namespace Funca.Benchmarks;

/// <summary>
/// Benchmarks for synchronous Result pipelines.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class ResultSyncBenchmarks
{
    private static readonly ErrorResult _validationError = ErrorResult.Validation("invalid");

    // ── Success chain ──────────────────────────────────────────────────────

    [Benchmark]
    public string SuccessChain_MapBindEnsureMatch()
    {
        return Result<int>.Ok(1)
            .Map(x => x + 1)
            .Bind(x => Result<int>.Ok(x * 2))
            .Ensure(x => x > 0, () => _validationError)
            .Match(
                onSuccess: x => $"ok:{x}",
                onFailure: _ => "fail");
    }

    // ── Early-failure chain ────────────────────────────────────────────────

    [Benchmark]
    public string FailureChain_EarlyExit()
    {
        return Result<int>.Error(_validationError)
            .Map(x => x + 1)               // skipped
            .Bind(x => Result<int>.Ok(x))  // skipped
            .Ensure(x => x > 0, () => _validationError) // skipped
            .Match(
                onSuccess: x => $"ok:{x}",
                onFailure: _ => "fail");
    }

    // ── Nullable-T success (tests _isSuccess flag independence) ───────────

    [Benchmark]
    public string NullableValueSuccess_IsOk()
    {
        var result = Result<string?>.Ok(null);   // null value, success = true
        return result.IsOk ? "ok" : "fail";
    }
}

using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultAsyncTests
{
    [Fact]
    public async Task Async_chain_awaits_callbacks_once_and_passes_nullable_values_and_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        List<string> calls = [];

        var validated = await Result<string?>.Of(null).EnsureAsync(async (value, token) =>
        {
            await Task.Yield();
            calls.Add("ensure");
            Assert.Null(value);
            Assert.Equal(cancellation.Token, token);

            return true;
        }, Error.Invalid("unexpected"), cancellation.Token);

        var result = await validated.BindAsync<string?>(async (value, token) =>
        {
            await Task.Yield();
            calls.Add("bind");
            Assert.Null(value);
            Assert.Equal(cancellation.Token, token);

            return Result<string?>.Of(null);
        }, cancellation.Token);

        var observed = await result.TapAsync(async (value, token) =>
        {
            await Task.Yield();
            calls.Add("tap");
            Assert.Null(value);
            Assert.Equal(cancellation.Token, token);
        }, cancellation.Token);

        Assert.Null(Assert.IsType<Success<string?>>(observed.Value).Value);
        Assert.Equal(new[] { "ensure", "bind", "tap" }, calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EnsureAsync_preserves_success_or_returns_the_supplied_error(bool passes)
    {
        var error = Error.Forbidden("custom validation");
        var calls = 0;

        var result = await Result<int>.Of(42).EnsureAsync(async (value, _) =>
        {
            await Task.Yield();
            calls++;
            Assert.Equal(42, value);

            return passes;
        }, error);

        if (passes)
            Assert.Equal(42, Assert.IsType<Success<int>>(result.Value).Value);
        else
            Assert.Equal(error, Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors));

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task EnsureAsync_uses_the_default_validation_error_when_none_is_supplied()
    {
        var result = await Result<int>.Of(0).EnsureAsync((value, _) => ValueTask.FromResult(value > 0));

        var error = Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors);
        Assert.Equal(ErrorType.Invalid, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Async_helpers_preserve_failures_without_running_callbacks(bool emptyErrors)
    {
        var errors = emptyErrors
            ? default(ErrorCollection)
            : new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);

        var validated = await Result<int>.Fail(errors).EnsureAsync(
            (_, _) => throw new Exception("The predicate must not run."), Error.Invalid("replacement"));
        var result = await validated.BindAsync<string>((_, _) => throw new Exception("The binder must not run."));
        var observed = await result.TapAsync((_, _) => throw new Exception("The action must not run."));

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(validated.Value));
        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
        Assert.Equal(errors, Assert.IsType<ErrorCollection>(observed.Value));
    }

    [Fact]
    public async Task BindAsync_propagates_errors_returned_by_the_binder()
    {
        var errors = new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);

        var result = await Result<int>.Of(42).BindAsync(async (value, _) =>
        {
            await Task.Yield();
            Assert.Equal(42, value);

            return Result<string>.Fail(errors);
        });

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Precancelled_helpers_do_not_run_callbacks_even_for_failure_results(bool failure)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var source = failure ? Result<int>.Fail(Error.Invalid("existing")) : Result<int>.Of(42);

        var ensureException = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await source.EnsureAsync((_, _) => throw new Exception("The predicate must not run."), cancellation.Token));
        var bindException = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await source.BindAsync<string>((_, _) => throw new Exception("The binder must not run."),
                cancellation.Token));

        var tapException = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await source.TapAsync((_, _) => throw new Exception("The action must not run."), cancellation.Token));

        Assert.Equal(cancellation.Token, ensureException.CancellationToken);
        Assert.Equal(cancellation.Token, bindException.CancellationToken);
        Assert.Equal(cancellation.Token, tapException.CancellationToken);
    }

    [Fact]
    public async Task EnsureAsync_observes_cancellation_while_awaiting_a_predicate_that_ignores_the_token()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = Result<int>.Of(42);

        var pending = source.EnsureAsync((_, _) => new ValueTask<bool>(completion.Task), cancellation.Token);
        Assert.False(pending.IsCompleted);
        cancellation.Cancel();
        completion.SetResult(false);

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.Equal(42, Assert.IsType<Success<int>>(source.Value).Value);
    }

    [Fact]
    public async Task BindAsync_observes_cancellation_while_awaiting_a_binder_that_ignores_the_token()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource<Result<string>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result<int>.Of(42)
            .BindAsync((_, _) => new ValueTask<Result<string>>(completion.Task), cancellation.Token);
        Assert.False(pending.IsCompleted);
        cancellation.Cancel();
        completion.SetResult(Result<string>.Of("completed"));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task TapAsync_awaits_the_action_once_and_preserves_the_original_success_value()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var value = new object();
        var calls = 0;

        var pending = Result<object>.Of(value).TapAsync((actual, token) =>
        {
            calls++;
            Assert.Same(value, actual);
            Assert.Equal(cancellation.Token, token);

            return new ValueTask(completion.Task);
        }, cancellation.Token);

        Assert.False(pending.IsCompleted);
        completion.SetResult();
        var result = await pending;

        Assert.Same(value, Assert.IsType<Success<object>>(result.Value).Value);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task TapAsync_observes_cancellation_while_awaiting_an_action_that_ignores_the_token()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result<int>.Of(42).TapAsync((_, _) => new ValueTask(completion.Task), cancellation.Token);
        Assert.False(pending.IsCompleted);
        cancellation.Cancel();
        completion.SetResult();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task Async_helpers_reject_null_callbacks()
    {
        var source = Result<int>.Of(42);

        var defaultEnsure =
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.EnsureAsync(null!));
        var customEnsure =
            await Assert.ThrowsAsync<ArgumentNullException>(async ()
                => await source.EnsureAsync(null!, Error.Invalid("custom")));
        var bind = await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.BindAsync<string>(null!));
        var tap = await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.TapAsync(null!));

        Assert.Equal("predicate", defaultEnsure.ParamName);
        Assert.Equal("predicate", customEnsure.ParamName);
        Assert.Equal("binder", bind.ParamName);
        Assert.Equal("action", tap.ParamName);
    }

    [Fact]
    public async Task Async_helpers_reject_uninitialized_inputs_without_running_callbacks()
    {
        var source = default(Result<int>);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.EnsureAsync((_, _) => throw new Exception("The predicate must not run.")));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.BindAsync<string>((_, _) => throw new Exception("The binder must not run.")));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.TapAsync((_, _) => throw new Exception("The action must not run.")));
    }

    [Fact]
    public async Task BindAsync_rejects_an_uninitialized_result_returned_by_the_binder()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await Result<int>.Of(42).BindAsync<string>(async (_, _) =>
            {
                await Task.Yield();

                return default;
            }));
    }

    [Fact]
    public async Task Async_helpers_propagate_callback_exceptions_without_converting_them_to_failures()
    {
        var exception = new FormatException("callback");
        var source = Result<int>.Of(42);

        var ensureException = await Assert.ThrowsAsync<FormatException>(async () =>
            await source.EnsureAsync(async (_, _) =>
            {
                await Task.Yield();

                throw exception;
            }));
        var bindException = await Assert.ThrowsAsync<FormatException>(async () =>
            await source.BindAsync<string>(async (_, _) =>
            {
                await Task.Yield();

                throw exception;
            }));

        var tapException = await Assert.ThrowsAsync<FormatException>(async () =>
            await source.TapAsync(async (_, _) =>
            {
                await Task.Yield();

                throw exception;
            }));

        Assert.Same(exception, ensureException);
        Assert.Same(exception, bindException);
        Assert.Same(exception, tapException);
    }
}
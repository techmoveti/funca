using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests;

public class ContainerTests
{
    [Fact]
    public void OptionSomeAndNoneBehavior()
    {
        var some = Option.Some("hello");
        Assert.True(some.IsSome);
        Assert.False(some.IsNone);
        Assert.Equal("hello", some.Unwrap());

        var none = Option.None<string>();
        Assert.True(none.IsNone);
        Assert.False(none.IsSome);
        Assert.Equal("fallback", none.UnwrapOr("fallback"));
        Assert.Null(none.UnwrapOrDefault());
    }

    [Fact]
    public void OptionBindMapAndMatchBehavior()
    {
        var option = Option.Some(5);

        var bindResult = option.Bind(x => Option.Some(x * 2));
        Assert.True(bindResult.IsSome);
        Assert.Equal(10, bindResult.Unwrap());

        var mapResult = option.Map(x => x + 3);
        Assert.True(mapResult.IsSome);
        Assert.Equal(8, mapResult.Unwrap());

        var matchResult = option.Match(
            some => some * 2,
            () => -1);
        Assert.Equal(10, matchResult);
    }

    [Fact]
    public void ResultOkAndErrorBehavior()
    {
        var ok = Result.Ok("value");
        Assert.True(ok.IsOk);
        Assert.False(ok.IsError);
        Assert.Equal("value", ok.Unwrap());

        var error = Result.Error<string>(ErrorResult.Validation("invalid"));
        Assert.True(error.IsError);
        Assert.False(error.IsOk);
        Assert.Single(error.Errors);
        Assert.Equal(ErrorType.Validation, error.Errors[0].Type);
    }

    [Fact]
    public void ResultMapBindAndMatchBehavior()
    {
        var ok = Result.Ok(4);
        var mapResult = ok.Map(x => x * 3);
        Assert.True(mapResult.IsOk);
        Assert.Equal(12, mapResult.Unwrap());

        var bindResult = ok.Bind(x => Result.Ok(x + 2));
        Assert.True(bindResult.IsOk);
        Assert.Equal(6, bindResult.Unwrap());

        var matchResult = ok.Match(
            success => success + 1,
            errors => -1);
        Assert.Equal(5, matchResult);
    }

    [Fact]
    public async Task ResultEnsureTaskBehavior()
    {
        var ok = await Result
            .Ok(4)
            .Ensure(
                x => Task.FromResult(x > 3),
                () => ErrorResult.Validation("invalid"));

        Assert.True(ok.IsOk);
        Assert.Equal(4, ok.Unwrap());

        var error = await Result
            .Ok(2)
            .Ensure(
                x => Task.FromResult(x > 3),
                () => ErrorResult.Validation("invalid"));

        Assert.True(error.IsError);
        Assert.Single(error.Errors);
        Assert.Equal("invalid", error.Errors[0].Message);
    }

    [Fact]
    public async Task ResultEnsureValueTaskBehavior()
    {
        var ok = await Result
            .Ok(4)
            .EnsureValueTask(
                x => ValueTask.FromResult(x > 3),
                () => ErrorResult.Validation("invalid"));

        Assert.True(ok.IsOk);
        Assert.Equal(4, ok.Unwrap());

        var error = await Result
            .Ok(2)
            .EnsureValueTask(
                x => ValueTask.FromResult(x > 3),
                () => ErrorResult.Validation("invalid"));

        Assert.True(error.IsError);
        Assert.Single(error.Errors);
        Assert.Equal("invalid", error.Errors[0].Message);
    }

    [Fact]
    public async Task ResultEnsureDoesNotRunConditionWhenAlreadyError()
    {
        var conditionWasCalled = false;

        var result = await Task
            .FromResult(Result.Error<int>(ErrorResult.Validation("original")))
            .Ensure(
                x =>
                {
                    conditionWasCalled = true;
                    return x > 0;
                },
                () => ErrorResult.Validation("new"));

        Assert.True(result.IsError);
        Assert.False(conditionWasCalled);
        Assert.Single(result.Errors);
        Assert.Equal("original", result.Errors[0].Message);
    }

    [Fact]
    public void ErrorResultFactoriesProduceExpectedValues()
    {
        var validation = ErrorResult.Validation("fail");
        Assert.Equal(ErrorType.Validation, validation.Type);
        Assert.Equal("fail", validation.Message);

        var notFound = ErrorResult.NotFound("missing");
        Assert.Equal(ErrorType.NotFound, notFound.Type);
        Assert.Equal("missing", notFound.Message);
    }
}

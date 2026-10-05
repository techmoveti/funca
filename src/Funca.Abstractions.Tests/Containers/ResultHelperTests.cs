using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultHelperTests
{
    [Fact]
    public void Match_handles_a_nullable_success_without_running_the_failure_handler()
    {
        var calls = 0;

        var output = Result<string?>.Of(null).Match(value =>
        {
            calls++;
            Assert.Null(value);

            return "success";
        }, _ => throw new Exception("The failure handler must not run."));

        Assert.Equal("success", output);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Match_handles_failures_without_running_the_success_handler(bool emptyErrors)
    {
        var errors = emptyErrors
            ? default(ErrorCollection)
            : new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);
        var calls = 0;

        var output = Result<int>.Fail(errors).Match<ErrorCollection>(
            _ => throw new Exception("The success handler must not run."),
            actual =>
            {
                calls++;

                return actual;
            });

        Assert.Equal(errors, output);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Tap_preserves_the_success_value_and_runs_once()
    {
        var value = new object();
        var calls = 0;

        var result = Result<object>.Of(value).Tap(actual =>
        {
            calls++;
            Assert.Same(value, actual);
        });

        Assert.Same(value, Assert.IsType<Success<object>>(result.Value).Value);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Tap_can_observe_a_nullable_success()
    {
        var observed = false;

        var result = Result<string?>.Of(null).Tap(value => observed = value is null);

        Assert.True(observed);
        Assert.Null(Assert.IsType<Success<string?>>(result.Value).Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tap_preserves_failures_without_running_the_action(bool emptyErrors)
    {
        var errors = emptyErrors ? default(ErrorCollection) : new ErrorCollection([Error.Invalid("existing")]);

        var result = Result<int>.Fail(errors).Tap(_ => throw new Exception("The action must not run."));

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
    }

    [Fact]
    public void Recover_preserves_success_without_running_the_recovery()
    {
        var value = new object();

        var result = Result<object>.Of(value).Recover(_ => throw new Exception("The recovery must not run."));

        Assert.Same(value, Assert.IsType<Success<object>>(result.Value).Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Recover_receives_errors_once_and_can_return_a_nullable_success(bool emptyErrors)
    {
        var errors = emptyErrors
            ? default(ErrorCollection)
            : new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);
        var calls = 0;

        var result = Result<string?>.Fail(errors).Recover(actual =>
        {
            calls++;
            Assert.Equal(errors, actual);

            return Result<string?>.Of(null);
        });

        Assert.Null(Assert.IsType<Success<string?>>(result.Value).Value);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Recover_can_return_a_new_failure()
    {
        var error = Error.NotFound("replacement");

        var result = Result<int>.Fail(Error.Invalid("original")).Recover(_ => Result<int>.Fail(error));

        Assert.Equal(error, Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors));
    }

    [Fact]
    public void Recover_rejects_an_uninitialized_result_returned_by_the_recovery()
    {
        Assert.Throws<InvalidOperationException>(()
            => Result<int>.Fail(Error.Invalid("original")).Recover(_ => default));
    }

    [Fact]
    public void Helpers_reject_null_delegates_even_when_their_branch_is_inactive()
    {
        var success = Result<int>.Of(42);
        var failure = Result<int>.Fail(Error.Invalid("failure"));

        Assert.Equal("onSuccess",
            Assert.Throws<ArgumentNullException>(() => failure.Match<int>(null!, _ => 0)).ParamName);
        Assert.Equal("onFailure",
            Assert.Throws<ArgumentNullException>(() => success.Match(value => value, null!)).ParamName);
        Assert.Equal("action", Assert.Throws<ArgumentNullException>(() => failure.Tap(null!)).ParamName);
        Assert.Equal("recovery", Assert.Throws<ArgumentNullException>(() => success.Recover(null!)).ParamName);
    }

    [Fact]
    public void Helpers_reject_uninitialized_inputs_without_running_delegates()
    {
        var result = default(Result<int>);

        Assert.Throws<InvalidOperationException>(() => result.Match<int>(
            _ => throw new Exception("The success handler must not run."),
            _ => throw new Exception("The failure handler must not run.")));
        Assert.Throws<InvalidOperationException>(()
            => result.Tap(_ => throw new Exception("The action must not run.")));
        Assert.Throws<InvalidOperationException>(()
            => result.Recover(_ => throw new Exception("The recovery must not run.")));
    }

    [Fact]
    public void Helpers_propagate_callback_exceptions_without_converting_them_to_failures()
    {
        var exception = new FormatException("callback");
        var success = Result<int>.Of(42);
        var failure = Result<int>.Fail(Error.Invalid("failure"));

        Assert.Same(exception, Assert.Throws<FormatException>(() => success.Match<int>(_ => throw exception, _ => 0)));
        Assert.Same(exception, Assert.Throws<FormatException>(() => failure.Match<int>(_ => 0, _ => throw exception)));
        Assert.Same(exception, Assert.Throws<FormatException>(() => success.Tap(_ => throw exception)));
        Assert.Same(exception, Assert.Throws<FormatException>(() => failure.Recover(_ => throw exception)));
    }
}
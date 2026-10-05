using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultTests
{
    [Fact]
    public void Fluent_chain_validates_binds_and_combines_values_of_different_types()
    {
        List<string> calls = [];

        Result<string> result = Result<int>.Of(42)
            .Ensure(value => value > 0, Error.Invalid("number", "Must be positive"))
            .Bind(value =>
            {
                calls.Add("bind");

                return Result<string>.Of(value.ToString());
            })
            .Combine(Result<bool>.Of(true), (text, enabled) =>
            {
                calls.Add("combine");

                return (text, enabled);
            })
            .Map(value => value.enabled ? value.text : "disabled");

        Assert.Equal("42", Assert.IsType<Success<string>>(result.Value).Value);
        Assert.Equal(new[] { "bind", "combine" }, calls);
    }

    [Fact]
    public void Ensure_preserves_the_custom_error_and_skips_later_operations()
    {
        var error = Error.Invalid("number", "Must be positive");

        var result = Result<int>.Of(0)
            .Ensure(value => value > 0, error)
            .Ensure(_ => throw new InvalidOperationException(), Error.Failure("Must not replace the error"))
            .Bind<string>(_ => throw new InvalidOperationException())
            .Map<int>(_ => throw new InvalidOperationException());

        Assert.Equal(error, Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Bind_preserves_existing_failures_without_running_the_binder(bool emptyErrors)
    {
        var errors = emptyErrors ? default(ErrorCollection) : new ErrorCollection([Error.Invalid("existing")]);

        var result = Result<int>.Fail(errors).Bind<string>(_ => throw new InvalidOperationException());

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
    }

    [Fact]
    public void Bind_propagates_a_failure_returned_by_the_binder()
    {
        var errors = new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);

        var result = Result<int>.Of(42)
            .Bind(value => value == 42 ? Result<string>.Fail(errors) : Result<string>.Of("unexpected"))
            .Map<int>(_ => throw new InvalidOperationException());

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
    }

    [Fact]
    public void Bind_supports_nullable_success_values()
    {
        var result = Result<string?>.Of(null)
            .Bind(value => Result<string?>.Of(value));

        Assert.Null(Assert.IsType<Success<string?>>(result.Value).Value);
    }

    [Fact]
    public void Bind_rejects_a_null_binder()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Result<int>.Of(42).Bind<string>(null!));

        Assert.Equal("binder", exception.ParamName);
    }

    [Fact]
    public void Bind_rejects_an_uninitialized_input_without_running_the_binder()
    {
        Assert.Throws<InvalidOperationException>(() => default(Result<int>)
            .Bind<string>(_ => throw new Exception("The binder must not run.")));
    }

    [Fact]
    public void Bind_rejects_an_uninitialized_result_returned_by_the_binder()
    {
        Assert.Throws<InvalidOperationException>(() => Result<int>.Of(42).Bind<string>(_ => default));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Generic_combine_propagates_errors_in_order_without_running_the_combiner(bool failLeft, bool failRight)
    {
        var first = Error.Invalid("first");
        var shared = Error.Invalid("shared");
        var last = Error.NotFound("last");
        var left = failLeft ? Result<int>.Fail(new ErrorCollection([first, shared])) : Result<int>.Of(42);
        var right = failRight ? Result<string>.Fail(new ErrorCollection([shared, last])) : Result<string>.Of("right");

        var result = left.Combine<string, bool>(right, (_, _) => throw new InvalidOperationException());

        Error[] expected = failLeft
            ? failRight ? [first, shared, shared, last] : [first, shared]
            : [shared, last];
        Assert.Equal(expected, Assert.IsType<ErrorCollection>(result.Value).Errors.ToArray());
    }

    [Fact]
    public void Generic_combine_preserves_an_empty_failure()
    {
        var result = Result<int>.Of(42)
            .Combine<string, bool>(Result<string>.Fail(default(ErrorCollection)),
                (_, _) => throw new InvalidOperationException());

        Assert.Empty(Assert.IsType<ErrorCollection>(result.Value).Errors);
    }

    [Fact]
    public void Generic_combine_supports_nullable_inputs_and_output()
    {
        var result = Result<string?>.Of(null).Combine(Result<int?>.Of(null), (text, number) =>
            text is null && number is null ? (string?)null : "unexpected");

        Assert.Null(Assert.IsType<Success<string?>>(result.Value).Value);
    }

    [Fact]
    public void Generic_combine_rejects_a_null_combiner()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Result<int>.Of(42)
            .Combine<string, bool>(Result<string>.Of("right"), null!));

        Assert.Equal("combiner", exception.ParamName);
    }

    [Fact]
    public void Generic_combine_rejects_an_uninitialized_instance_without_running_the_combiner()
    {
        Assert.Throws<InvalidOperationException>(() => default(Result<int>)
            .Combine<string, bool>(Result<string>.Of("right"),
                (_, _) => throw new Exception("The combiner must not run.")));
    }

    [Fact]
    public void Generic_combine_rejects_an_uninitialized_other_result_without_running_the_combiner()
    {
        var exception = Assert.Throws<ArgumentException>(() => Result<int>.Of(42)
            .Combine<string, bool>(default, (_, _) => throw new Exception("The combiner must not run.")));

        Assert.Equal("other", exception.ParamName);
    }

    [Fact]
    public void Fluent_chain_validates_combines_and_maps_the_original_success_value_once()
    {
        var calls = 0;

        Result<string> result = Result<int>.Of(42)
            .Ensure(value => value > 0)
            .Ensure(value => value < 100)
            .Combine(Result<int>.Of(99))
            .Map(value =>
            {
                calls++;

                return value.ToString();
            });

        Assert.Equal("42", Assert.IsType<Success<string>>(result.Value).Value);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Fluent_chain_skips_later_predicates_and_mapping_after_a_failed_validation()
    {
        var result = Result<int>.Of(0)
            .Ensure(value => value > 0)
            .Ensure(_ => throw new InvalidOperationException())
            .Map<string>(_ => throw new InvalidOperationException());

        Assert.Equal(ErrorType.Invalid, Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors).Type);
    }

    [Fact]
    public void Instance_combine_accumulates_errors_and_map_preserves_them_without_running_the_mapper()
    {
        var first = Error.Invalid("first");
        var second = Error.NotFound("second");

        var result = Result<int>.Fail(first)
            .Combine(Result<int>.Fail(second))
            .Map<string>(_ => throw new InvalidOperationException());

        Assert.Equal(new[] { first, second }, Assert.IsType<ErrorCollection>(result.Value).Errors.ToArray());
    }

    [Fact]
    public void Instance_combine_propagates_a_failure_from_the_other_result()
    {
        var error = Error.Invalid("other");

        var result = Result<int>.Of(42)
            .Combine(Result<int>.Fail(error))
            .Map<string>(_ => throw new InvalidOperationException());

        Assert.Equal(error, Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors));
    }

    [Fact]
    public void Map_preserves_an_empty_failure_without_running_the_mapper()
    {
        var result = Result<int>.Fail(default(ErrorCollection))
            .Map<string>(_ => throw new InvalidOperationException());

        Assert.Empty(Assert.IsType<ErrorCollection>(result.Value).Errors);
    }

    [Fact]
    public void Map_supports_nullable_input_and_output_values()
    {
        var fromNull = Result<string?>.Of(null).Map(value => value is null ? 42 : 0);
        var toNull = Result<int>.Of(42).Map<string?>(_ => null);

        Assert.Equal(42, Assert.IsType<Success<int>>(fromNull.Value).Value);
        Assert.Null(Assert.IsType<Success<string?>>(toNull.Value).Value);
    }

    [Fact]
    public void Map_wraps_an_error_collection_payload_as_a_success()
    {
        var payload = new ErrorCollection([Error.Invalid("payload")]);

        var result = Result<int>.Of(42).Map(_ => payload);

        Assert.Equal(payload, Assert.IsType<Success<ErrorCollection>>(result.Value).Value);
    }

    [Fact]
    public void Map_rejects_a_null_mapper()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Result<int>.Of(42).Map<string>(null!));

        Assert.Equal("mapper", exception.ParamName);
    }

    [Fact]
    public void Map_rejects_an_uninitialized_result_without_running_the_mapper()
    {
        Assert.Throws<InvalidOperationException>(() => default(Result<int>)
            .Map<string>(_ => throw new Exception("The mapper must not run.")));
    }

    [Fact]
    public void Ensure_validates_the_success_value_once_and_preserves_it()
    {
        var value = new object();
        var calls = 0;

        var result = Result<object>.Of(value).Ensure(actual =>
        {
            Assert.Same(value, actual);
            calls++;

            return true;
        });

        Assert.Equal(1, calls);
        Assert.Same(value, Assert.IsType<Success<object>>(result.Value).Value);
    }

    [Fact]
    public void Ensure_returns_an_invalid_error_when_validation_fails()
    {
        var result = Result<int>.Of(0).Ensure(value => value > 0);

        var error = Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors);
        Assert.Equal(ErrorType.Invalid, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Ensure_preserves_existing_failures_without_evaluating_the_predicate(bool emptyErrors)
    {
        var errors = emptyErrors
            ? default(ErrorCollection)
            : new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);

        var result = Result<int>.Fail(errors).Ensure(_ => throw new InvalidOperationException());

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
    }

    [Fact]
    public void Ensure_can_validate_a_nullable_success()
    {
        var result = Result<string?>.Of(null).Ensure(value => value is null);

        Assert.Null(Assert.IsType<Success<string?>>(result.Value).Value);
    }

    [Fact]
    public void Ensure_rejects_a_null_predicate()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Result<int>.Of(1).Ensure(null!));

        Assert.Equal("predicate", exception.ParamName);
    }

    [Fact]
    public void Ensure_rejects_an_uninitialized_result_without_evaluating_the_predicate()
    {
        var predicateCalled = false;

        Assert.Throws<InvalidOperationException>(() => default(Result<int>).Ensure(_ =>
        {
            predicateCalled = true;

            return true;
        }));

        Assert.False(predicateCalled);
    }

    [Fact]
    public void Combine_preserves_the_left_success_value()
    {
        var result = Result<int>.Combine(Result<int>.Ok(1), Result<int>.Ok(2));

        Assert.Equal(1, Assert.IsType<Success<int>>(result.Value).Value);
    }

    [Fact]
    public void Combine_preserves_a_nullable_success_value()
    {
        var result = Result<string?>.Combine(Result<string?>.Ok(null), Result<string?>.Ok("right"));

        Assert.Null(Assert.IsType<Success<string?>>(result.Value).Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Combine_preserves_a_failure_on_either_side(bool failureOnLeft)
    {
        var errors = new ErrorCollection([Error.Invalid("first"), Error.NotFound("second")]);
        var failure = Result<int>.Fail(errors);
        var success = Result<int>.Ok(42);

        var result = failureOnLeft
            ? Result<int>.Combine(failure, success)
            : Result<int>.Combine(success, failure);

        Assert.Equal(errors, Assert.IsType<ErrorCollection>(result.Value));
    }

    [Fact]
    public void Combine_accumulates_errors_in_order_without_removing_duplicates()
    {
        var first = Error.Invalid("first");
        var second = Error.NotFound("second");
        var left = Result<int>.Fail(new ErrorCollection([first, second]));
        var right = Result<int>.Fail(new ErrorCollection([first]));

        var result = Result<int>.Combine(left, right);

        Assert.Equal(new[] { first, second, first }, Assert.IsType<ErrorCollection>(result.Value).Errors.ToArray());
        Assert.Equal(new[] { first, second }, Assert.IsType<ErrorCollection>(left.Value).Errors.ToArray());
        Assert.Equal(first, Assert.Single(Assert.IsType<ErrorCollection>(right.Value).Errors));
    }

    [Fact]
    public void Combine_preserves_an_empty_failure()
    {
        var failure = Result<int>.Fail(default(ErrorCollection));

        var result = Result<int>.Combine(Result<int>.Ok(42), failure);
        var combinedFailures = Result<int>.Combine(failure, failure);

        Assert.Empty(Assert.IsType<ErrorCollection>(result.Value).Errors);
        Assert.Empty(Assert.IsType<ErrorCollection>(combinedFailures.Value).Errors);
    }

    [Theory]
    [InlineData(true, false, "left")]
    [InlineData(false, true, "right")]
    [InlineData(true, true, "left")]
    public void Combine_rejects_uninitialized_results(bool defaultLeft, bool defaultRight, string parameter)
    {
        var left = defaultLeft ? default : Result<int>.Ok(1);
        var right = defaultRight ? default : Result<int>.Ok(2);

        var exception = Assert.Throws<ArgumentException>(() => Result<int>.Combine(left, right));

        Assert.Equal(parameter, exception.ParamName);
    }
}
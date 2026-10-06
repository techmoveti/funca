using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultBuilderTests
{
    [Fact]
    public void Build_supports_every_arity_and_preserves_the_order_of_repeated_types()
    {
        var one = ResultBuilder.Combine().Add(Result<int>.Ok(1));
        var two = one.Add(Result<int>.Ok(2));
        var three = two.Add(Result<int>.Ok(3));
        var four = three.Add(Result<int>.Ok(4));
        var five = four.Add(Result<int>.Ok(5));
        var six = five.Add(Result<int>.Ok(6));
        var seven = six.Add(Result<int>.Ok(7));
        var eight = seven.Add(Result<int>.Ok(8));

        Assert.Equal(1, one.Build(a => a).Unwrap());
        Assert.Equal((1, 2), two.Build((a, b) => (a, b)).Unwrap());
        Assert.Equal((1, 2, 3), three.Build((a, b, c) => (a, b, c)).Unwrap());
        Assert.Equal((1, 2, 3, 4), four.Build((a, b, c, d) => (a, b, c, d)).Unwrap());
        Assert.Equal((1, 2, 3, 4, 5), five.Build((a, b, c, d, e) => (a, b, c, d, e)).Unwrap());
        Assert.Equal((1, 2, 3, 4, 5, 6), six.Build((a, b, c, d, e, f) => (a, b, c, d, e, f)).Unwrap());
        Assert.Equal((1, 2, 3, 4, 5, 6, 7), seven.Build((a, b, c, d, e, f, g) => (a, b, c, d, e, f, g)).Unwrap());
        Assert.Equal((1, 2, 3, 4, 5, 6, 7, 8),
            eight.Build((a, b, c, d, e, f, g, h) => (a, b, c, d, e, f, g, h)).Unwrap());
    }

    [Fact]
    public void Build_preserves_different_types_references_and_nullable_successes_and_runs_once()
    {
        var reference = new object();
        var calls = 0;

        var result = ResultBuilder.Combine()
            .Add(Result<object>.Ok(reference))
            .Add(Result<int>.Ok(42))
            .Add(Result<string?>.Ok(null))
            .Add(Result<int?>.Ok(null))
            .Build((value, number, text, optionalNumber) =>
            {
                calls++;
                Assert.Same(reference, value);
                Assert.Equal(42, number);
                Assert.Null(text);
                Assert.Null(optionalNumber);

                return (string?)null;
            });

        Assert.Null(result.Unwrap());
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Add_evaluates_all_inputs_and_accumulates_errors_in_order_including_duplicates()
    {
        var calls = new List<int>();
        var first = Error.Invalid("first");
        var shared = Error.Invalid("shared");
        var last = Error.NotFound("last");

        Result<int> Validate(int position, ErrorCollection? errors = null)
        {
            calls.Add(position);

            return errors.HasValue ? Result<int>.Fail(errors.Value) : Result<int>.Ok(position);
        }

        var result = ResultBuilder.Combine()
            .Add(Validate(1, new ErrorCollection([first, shared])))
            .Add(Validate(2))
            .Add(Validate(3, new ErrorCollection([shared])))
            .Add(Validate(4, default(ErrorCollection)))
            .Add(Validate(5))
            .Add(Validate(6))
            .Add(Validate(7))
            .Add(Validate(8, new ErrorCollection([last])))
            .Build<int>((_, _, _, _, _, _, _, _) => throw new Exception("The factory must not run."));

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, calls);
        Assert.Equal(new[] { first, shared, shared, last },
            Assert.IsType<ErrorCollection>(result.Value).Errors.ToArray());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void Add_preserves_an_empty_failure_at_any_position(int failurePosition)
    {
        Result<int> Input(int position) => position == failurePosition
            ? Result<int>.Fail(default(ErrorCollection))
            : Result<int>.Ok(position);

        var result = ResultBuilder.Combine()
            .Add(Input(1)).Add(Input(2)).Add(Input(3)).Add(Input(4))
            .Add(Input(5)).Add(Input(6)).Add(Input(7)).Add(Input(8))
            .Build<int>((_, _, _, _, _, _, _, _) => throw new Exception("The factory must not run."));

        Assert.Empty(Assert.IsType<ErrorCollection>(result.Value).Errors);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void Add_rejects_an_uninitialized_input_even_after_failure(int uninitializedPosition)
    {
        Result<int> Input(int position) => position == uninitializedPosition
            ? default
            : Result<int>.Fail(Error.Invalid("existing"));

        Assert.Throws<ArgumentException>(() => ResultBuilder.Combine()
            .Add(Input(1)).Add(Input(2)).Add(Input(3)).Add(Input(4))
            .Add(Input(5)).Add(Input(6)).Add(Input(7)).Add(Input(8)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Every_build_arity_rejects_null_factories_even_on_failure(bool success)
    {
        var one = ResultBuilder.Combine().Add(success
            ? Result<int>.Ok(1)
            : Result<int>.Fail(Error.Invalid("existing")));
        var two = one.Add(Result<int>.Ok(2));
        var three = two.Add(Result<int>.Ok(3));
        var four = three.Add(Result<int>.Ok(4));
        var five = four.Add(Result<int>.Ok(5));
        var six = five.Add(Result<int>.Ok(6));
        var seven = six.Add(Result<int>.Ok(7));
        var eight = seven.Add(Result<int>.Ok(8));

        Action[] builds =
        [
            () => one.Build<int>(null!),
            () => two.Build<int>(null!),
            () => three.Build<int>(null!),
            () => four.Build<int>(null!),
            () => five.Build<int>(null!),
            () => six.Build<int>(null!),
            () => seven.Build<int>(null!),
            () => eight.Build<int>(null!)
        ];

        foreach (var build in builds)
            Assert.Equal("factory", Assert.Throws<ArgumentNullException>(build).ParamName);
    }

    [Fact]
    public void Default_typed_builders_reject_build_and_add()
    {
        Assert.Throws<InvalidOperationException>(() => default(ResultBuilder<int>).Build(value => value));
        Assert.Throws<InvalidOperationException>(() => default(ResultBuilder<int>).Add(Result<string>.Ok("text")));
        Assert.Throws<InvalidOperationException>(() => default(ResultBuilder<int, string>).Build((a, b) => (a, b)));
        Assert.Throws<InvalidOperationException>(() => default(ResultBuilder<int, string>).Add(Result<bool>.Ok(true)));
    }

    [Fact]
    public void Builder_can_be_branched_without_changing_previously_added_values()
    {
        var original = ResultBuilder.Combine().Add(Result<int>.Ok(42));
        var successBranch = original.Add(Result<string>.Ok("success"));
        var error = Error.Invalid("other branch");
        var failureBranch = original.Add(Result<string>.Fail(error));

        Assert.Equal(42, original.Build(value => value).Unwrap());
        Assert.Equal((42, "success"), successBranch.Build((number, text) => (number, text)).Unwrap());
        Assert.Equal(error, Assert.Single(Assert.IsType<ErrorCollection>(
            failureBranch.Build((number, text) => (number, text)).Value).Errors));
    }

    [Fact]
    public void Build_propagates_factory_exceptions()
    {
        var exception = new FormatException("factory");
        var builder = ResultBuilder.Combine().Add(Result<int>.Ok(1)).Add(Result<string>.Ok("text"));

        Assert.Same(exception, Assert.Throws<FormatException>(() =>
            builder.Build<int>((_, _) => throw exception)));
    }
}
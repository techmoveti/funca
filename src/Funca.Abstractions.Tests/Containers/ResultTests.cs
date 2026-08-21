using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultTests
{
    [Fact]
    public void Successful_result_does_not_expose_errors()
    {
        var result = Result<int>.Wrap(1);

        Assert.True(result.Errors.IsEmpty);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Error_requires_at_least_one_non_null_error()
    {
        Assert.Throws<ArgumentException>(() => Result<int>.Error([]));
        Assert.Throws<ArgumentNullException>(() => Result<int>.Error([null!]));
    }

    [Fact]
    public void Error_does_not_retain_or_expose_the_caller_array()
    {
        var original = ErrorResult.Validation("original");
        var input = new[] { original };
        var result = Result<int>.Error(input);

        input[0] = ErrorResult.Failure("mutated input");
        var materialized = result.ErrorsToArray();
        materialized[0] = ErrorResult.Failure("mutated output");

        Assert.Equal(original, result.Errors[0]);
    }

    [Fact]
    public void Failed_result_is_propagated_without_changing_its_error()
    {
        var error = ErrorResult.Validation("invalid value");
        var result = Result<int>.Error(error);

        var mapped = result.Map(value => value.ToString());

        Assert.True(mapped.IsError);
        Assert.Equal(error, mapped.Errors[0]);
    }

    [Fact]
    public void Default_result_exposes_a_diagnostic_error()
    {
        Result<int> result = default;

        Assert.True(result.IsError);
        Assert.True(result.HasErrors);
        Assert.Equal("Result was not initialized.", result.Errors[0].Message);
    }
}
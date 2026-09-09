using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultBuilderTests
{
    [Fact]
    public void Build_returns_factory_value_when_all_validations_succeed()
    {
        var result = new ResultBuilder()
            .Add(Result.Ok(10))
            .Add("Name", Result.Ok("funca"))
            .Build(builder => $"{builder.Get<int>()}:{builder.Get<string>("Name")}");

        Assert.True(result.IsOk);
        Assert.Equal("10:funca", result.Unwrap());
    }

    [Fact]
    public void Build_aggregates_validation_errors_and_does_not_call_factory()
    {
        var firstError = ErrorResult.Invalid("first");
        var secondError = ErrorResult.NotFound("second");
        var factoryWasCalled = false;

        var result = new ResultBuilder()
            .Add(Result.Error<int>(firstError))
            .Add("Name", Result.Error<string>(secondError))
            .Build(_ =>
            {
                factoryWasCalled = true;

                return 1;
            });

        Assert.False(factoryWasCalled);
        Assert.True(result.IsError);
        Assert.Equal([firstError, secondError], result.ErrorsToArray());
    }

    [Fact]
    public void Collection_validation_stores_success_values()
    {
        var result = new ResultBuilder()
            .Add("Numbers", [Result.Ok(1), Result.Ok(2), Result.Ok(3)])
            .Build(builder => builder.Get<IEnumerable<int>>("Numbers").Sum());

        Assert.True(result.IsOk);
        Assert.Equal(6, result.Unwrap());
    }

    [Fact]
    public void Collection_validation_aggregates_all_errors()
    {
        var firstError = ErrorResult.Invalid("first");
        var secondError = ErrorResult.Forbidden("second");

        var result = new ResultBuilder()
            .Add("Numbers", [Result.Ok(1), Result.Error<int>(firstError), Result.Error<int>(secondError)])
            .Build(_ => 1);

        Assert.True(result.IsError);
        Assert.Equal([firstError, secondError], result.ErrorsToArray());
    }

    [Fact]
    public void Result_factory_build_returns_factory_result()
    {
        var error = ErrorResult.Invalid("invalid aggregate");

        var result = new ResultBuilder()
            .Add(Result.Ok(1))
            .Build(_ => Result.Error<string>(error));

        Assert.True(result.IsError);
        Assert.Equal(error, result.Errors[0]);
    }
}
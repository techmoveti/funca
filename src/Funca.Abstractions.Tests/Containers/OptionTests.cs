using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class OptionTests
{
    [Fact]
    public void None_is_not_some_for_value_types()
    {
        var option = Option.None<int>();

        Assert.True(option.IsNone);
        Assert.False(option.IsSome);
    }

    [Fact]
    public void ToResult_without_explicit_error_returns_a_diagnostic_error()
    {
        var result = Option.None<int>().ToResult();

        Assert.True(result.IsError);
        Assert.Equal("Option does not contain a value.", result.Errors[0].Message);
    }
}
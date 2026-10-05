using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ErrorCollectionTests
{
    [Fact]
    public void Errors_are_snapshots_of_mutable_inputs()
    {
        var original = Error.Invalid("original");
        Error[] array = [original];
        List<Error> list = [original];
        ErrorCollection fromArray = array;
        ErrorCollection fromList = list;

        array[0] = Error.Invalid("changed");
        list.Clear();

        Assert.Equal(original, Assert.Single(fromArray.Errors));
        Assert.Equal(original, Assert.Single(fromList.Errors));
    }

    [Fact]
    public void Default_collection_is_empty_and_null_input_is_rejected()
    {
        Assert.Empty(default(ErrorCollection).Errors);
        Assert.Throws<ArgumentNullException>(() => new ErrorCollection(null!));
    }

    [Fact]
    public void Failure_factory_accepts_a_single_error_without_chained_conversions()
    {
        var error = Error.Invalid("failure");
        var result = Result<int>.Fail(error);

        Assert.Equal(error, Assert.Single(Assert.IsType<ErrorCollection>(result.Value).Errors));
    }
}
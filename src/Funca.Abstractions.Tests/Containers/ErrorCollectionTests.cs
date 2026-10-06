using System.Collections.Immutable;
using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ErrorCollectionTests
{
    [Fact]
    public void Immutable_inputs_are_reused_and_default_arrays_are_normalized()
    {
        var source = ImmutableArray.Create(Error.Invalid("first"), Error.NotFound("second"));
        var collection = new ErrorCollection(source);
        var empty = new ErrorCollection(default(ImmutableArray<Error>));

        Assert.True(source == collection.Errors);
        Assert.False(empty.Errors.IsDefault);
        Assert.Empty(empty.Errors);
    }

    [Fact]
    public void Single_error_construction_and_factories_preserve_the_error()
    {
        var error = Error.Invalid("field", "failure");
        ErrorCollection converted = error;

        Assert.Equal(error, Assert.Single(new ErrorCollection(error).Errors));
        Assert.Equal(error, Assert.Single(converted.Errors));
        Assert.Equal(error, Assert.Single(ResultModule.Fail(error).Errors));
        Assert.Equal(Error.Invalid("failure"), Assert.Single(ResultModule.Fail("failure").Errors));
    }

    [Fact]
    public void Result_module_takes_a_snapshot_of_array_inputs_and_rejects_null()
    {
        var original = Error.Invalid("original");
        Error[] array = [original];
        var collection = ResultModule.Fail(array);

        array[0] = Error.Invalid("changed");

        Assert.Equal(original, Assert.Single(collection.Errors));
        Assert.Throws<ArgumentNullException>(() => ResultModule.Fail((Error[])null!));
    }

    [Fact]
    public void Equality_and_hashing_use_ordered_contents_for_independent_arrays()
    {
        var first = new ErrorCollection(ImmutableArray.Create(Error.Invalid("first"), Error.NotFound("second")));
        var second = new ErrorCollection(ImmutableArray.Create(Error.Invalid("first"), Error.NotFound("second")));

        Assert.False(first.Errors == second.Errors);
        Assert.True(first == second);
        Assert.False(first != second);
        Assert.True(first.Equals(second));
        Assert.True(first.Equals((object)second));
        Assert.False(first.Equals("not a collection"));
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Single(new HashSet<ErrorCollection> { first, second });
    }

    [Fact]
    public void Equality_distinguishes_order_duplicates_and_error_content()
    {
        var first = Error.Invalid("first");
        var second = Error.NotFound("second");
        var collection = new ErrorCollection([first, second]);

        Assert.True(collection != new ErrorCollection([second, first]));
        Assert.True(collection != new ErrorCollection([first, second, second]));
        Assert.True(collection != new ErrorCollection([first, Error.NotFound("changed")]));
        Assert.True(collection != new ErrorCollection([first]));
    }

    [Fact]
    public void All_empty_representations_are_equal_and_have_the_same_hash()
    {
        var uninitialized = default(ErrorCollection);
        var empty = new ErrorCollection([]);
        var defaultArray = new ErrorCollection(default(ImmutableArray<Error>));
        var enumerable = new ErrorCollection(Enumerable.Empty<Error>());

        Assert.True(uninitialized == empty);
        Assert.True(uninitialized == defaultArray);
        Assert.True(uninitialized == enumerable);
        Assert.Equal(uninitialized.GetHashCode(), empty.GetHashCode());
        Assert.Single(new HashSet<ErrorCollection> { uninitialized, empty, defaultArray, enumerable });
        Assert.True(uninitialized != new ErrorCollection(Error.Empty));
    }

    [Fact]
    public void Combine_preserves_order_duplicates_and_original_inputs()
    {
        var first = Error.Invalid("first");
        var second = Error.NotFound("second");
        var left = new ErrorCollection([first, second]);
        var right = new ErrorCollection([first]);

        var combined = left.Combine(right);

        Assert.Equal(new[] { first, second, first }, combined.Errors.ToArray());
        Assert.Equal(new[] { first, second }, left.Errors.ToArray());
        Assert.Equal(first, Assert.Single(right.Errors));
    }

    [Fact]
    public void Combine_reuses_existing_arrays_when_either_collection_is_empty()
    {
        var collection = new ErrorCollection(Error.Invalid("failure"));
        var empty = default(ErrorCollection);

        Assert.True(collection.Errors == collection.Combine(empty).Errors);
        Assert.True(collection.Errors == empty.Combine(collection).Errors);
        Assert.Empty(empty.Combine(empty).Errors);
    }

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

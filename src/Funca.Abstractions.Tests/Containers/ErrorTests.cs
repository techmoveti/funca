using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ErrorTests
{
    [Fact]
    public void Default_error_is_empty_and_exposes_a_non_null_message()
    {
        var error = default(Error);
        var (key, type, message) = error;

        Assert.True(error.IsEmpty());
        Assert.True(error == Error.Empty);
        Assert.Null(key);
        Assert.Equal(ErrorType.Failure, type);
        Assert.Equal(string.Empty, message);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void Explicit_empty_message_and_default_error_have_consistent_equality_and_hashing()
    {
        var empty = new Error(null, ErrorType.Failure, string.Empty);

        Assert.True(empty == default(Error));
        Assert.True(empty.Equals((object)Error.Empty));
        Assert.Equal(empty.GetHashCode(), Error.Empty.GetHashCode());
        Assert.Single(new HashSet<Error> { empty, Error.Empty });
    }

    [Fact]
    public void IsEmpty_accepts_an_empty_key_but_preserves_meaningful_error_fields()
    {
        Assert.True(new Error(string.Empty, ErrorType.Failure, string.Empty).IsEmpty());
        Assert.False(new Error("field", ErrorType.Failure, string.Empty).IsEmpty());
        Assert.False(Error.Invalid(string.Empty).IsEmpty());
        Assert.False(Error.Failure().IsEmpty());
    }

    [Fact]
    public void Equality_and_hashing_preserve_key_type_and_message()
    {
        var error = Error.Invalid("field", "failure");
        var equivalent = new Error("field", ErrorType.Invalid, "failure");

        Assert.True(error == equivalent);
        Assert.Equal(error.GetHashCode(), equivalent.GetHashCode());
        Assert.True(error != error with { Key = "another field" });
        Assert.True(error != error with { Type = ErrorType.Failure });
        Assert.True(error != error with { Message = "another message" });
    }

    [Fact]
    public void Message_normalization_applies_to_construction_and_with_expressions()
    {
        var constructed = new Error(null, ErrorType.Failure, null!);
        var changed = Error.Failure("failure") with { Message = null! };

        Assert.Equal(string.Empty, constructed.Message);
        Assert.Equal(string.Empty, changed.Message);
        Assert.True(constructed == Error.Empty);
        Assert.True(changed == Error.Empty);
    }
}
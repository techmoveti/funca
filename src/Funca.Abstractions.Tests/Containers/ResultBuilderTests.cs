using Funca.Abstractions.Containers;

namespace Funca.Abstractions.Tests.Containers;

public sealed class ResultBuilderTests
{
    [Fact]
    public void Nullable_success_is_distinct_from_an_uninitialized_result()
    {
        var result = ResultBuilder.Combine().Build<string?>(_ => null);
        var success = Assert.IsType<Success<string?>>(result.Value);

        Assert.Null(success.Value);
        Assert.Null(default(Result<string?>).Value);
    }

    [Fact]
    public void ErrorCollection_can_be_a_success_value_without_becoming_a_failure()
    {
        var value = new ErrorCollection([Error.Invalid("payload")]);
        var result = ResultBuilder.Combine().Build<object>(_ => value);

        var success = Assert.IsType<Success<object>>(result.Value);
        Assert.Equal(value, Assert.IsType<ErrorCollection>(success.Value));
    }

    [Fact]
    public void Type_registration_distinguishes_closed_generic_types_and_explicit_aliases()
    {
        List<int> numbers = [1];
        List<string> words = ["word"];
        var builder = ResultBuilder.Combine()
            .Ensure(numbers, _ => true, "numbers")
            .Ensure(words, _ => true, "words")
            .Ensure(typeof(List<int>).Name, "alias", _ => true, "alias");

        Assert.Same(numbers, builder.Get<List<int>>());
        Assert.Same(words, builder.Get<List<string>>());
        Assert.Equal("alias", builder.Get<string>(typeof(List<int>).Name));
    }

    [Fact]
    public void Repeated_alias_replaces_the_value_and_failed_revalidation_removes_it()
    {
        var builder = ResultBuilder.Combine()
            .Ensure("name", "first", _ => true, "name")
            .Ensure("name", "second", _ => true, "name");

        Assert.Equal("second", builder.Get<string>("name"));

        builder.Ensure("name", "invalid", _ => false, "invalid");

        Assert.False(builder.IsValid);
        Assert.Throws<InvalidOperationException>(() => builder.Get<string>("name"));
    }

    [Fact]
    public void Invalid_build_accumulates_errors_without_invoking_the_factory_and_returns_a_snapshot()
    {
        var first = Error.Invalid("first");
        var second = Error.Invalid("second");
        var factoryCalled = false;
        var builder = ResultBuilder.Combine()
            .Ensure("first", 0, _ => false, first)
            .Ensure("second", 0, _ => false, second);

        var result = builder.Build(_ =>
        {
            factoryCalled = true;

            return 42;
        });

        builder.Ensure("third", 0, _ => false, "third");
        var failure = Assert.IsType<ErrorCollection>(result.Value);

        Assert.False(factoryCalled);
        Assert.Equal(new[] { first, second }, failure.Errors.ToArray());
        Assert.Equal(3, builder.GetErrors().Length);
    }

    [Fact]
    public void Validated_null_can_be_retrieved_and_incompatible_types_report_a_clear_error()
    {
        var builder = ResultBuilder.Combine()
            .Ensure<string?>("nullable", null, _ => true, "nullable")
            .Ensure("count", 1, _ => true, "count");

        Assert.Null(builder.Get<string?>("nullable"));
        Assert.Throws<InvalidOperationException>(() => builder.Get<int>("nullable"));
        var exception = Assert.Throws<InvalidOperationException>(() => builder.Get<string>("count"));
        Assert.Contains("count", exception.Message);
        Assert.Contains("System.String", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Invalid_alias_is_rejected_before_evaluating_the_validator(string alias)
    {
        var builder = ResultBuilder.Combine();
        var validatorCalled = false;

        Assert.Throws<ArgumentException>(() => builder.Ensure(alias, 1, _ =>
        {
            validatorCalled = true;

            return true;
        }, "invalid alias"));

        await Assert.ThrowsAsync<ArgumentException>(async () => await builder.EnsureAsync(alias, 1, (_, _) =>
        {
            validatorCalled = true;

            return ValueTask.FromResult(true);
        }, Error.Invalid("invalid alias")));

        Assert.False(validatorCalled);
    }

    [Fact]
    public async Task Null_aliases_and_delegates_are_rejected()
    {
        var builder = ResultBuilder.Combine();

        Assert.Throws<ArgumentNullException>(() => builder.Ensure(null!, 1, _ => true, "alias"));
        Assert.Throws<ArgumentNullException>(() => builder.Ensure("count", 1, null!, "validator"));
        Assert.Throws<ArgumentNullException>(() => builder.Ensure(1, null!, "validator"));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await builder.EnsureAsync("count", 1, null!, Error.Invalid("validator")));

        builder.Ensure(1, _ => false, "invalid");
        Assert.Throws<ArgumentNullException>(() => builder.Build<int>(null!));
    }

    [Fact]
    public async Task Async_validation_registers_success_and_removes_a_failed_revalidation()
    {
        var builder = ResultBuilder.Combine();
        using var cancellation = new CancellationTokenSource();

        await builder.EnsureAsync("count", 42, async (value, token) =>
        {
            await Task.Yield();
            Assert.Equal(cancellation.Token, token);

            return value == 42;
        }, Error.Invalid("count"), cancellation.Token);

        Assert.Equal(42, builder.Get<int>("count"));

        await builder.EnsureAsync("count", 0, (_, _) => ValueTask.FromResult(false), Error.Invalid("count"));

        Assert.False(builder.IsValid);
        Assert.Throws<InvalidOperationException>(() => builder.Get<int>("count"));
    }

    [Fact]
    public async Task Precancelled_validation_does_not_invoke_the_validator()
    {
        var builder = ResultBuilder.Combine();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var validatorCalled = false;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await builder.EnsureAsync("count", 1, (_, _) =>
            {
                validatorCalled = true;

                return ValueTask.FromResult(true);
            }, Error.Invalid("count"), cancellation.Token));

        Assert.False(validatorCalled);
        Assert.True(builder.IsValid);
        Assert.Throws<InvalidOperationException>(() => builder.Get<int>("count"));
    }

    [Fact]
    public async Task Cancellation_during_validation_does_not_commit_its_result()
    {
        var builder = ResultBuilder.Combine().Ensure("count", 42, _ => true, "count");
        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await builder.EnsureAsync("count", 0, (_, _) =>
            {
                cancellation.Cancel();

                return ValueTask.FromResult(false);
            }, Error.Invalid("count"), cancellation.Token));

        Assert.True(builder.IsValid);
        Assert.Equal(42, builder.Get<int>("count"));
    }
}
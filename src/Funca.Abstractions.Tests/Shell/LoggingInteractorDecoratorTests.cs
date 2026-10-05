using Funca.Abstractions.Containers;
using Funca.Abstractions.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Funca.Abstractions.Tests.Shell;

public sealed class LoggingInteractorDecoratorTests
{
    [Fact]
    public async Task Union_failure_is_logged_as_a_warning()
    {
        var result = Result<string?>.Fail(Error.Invalid("failure"));
        var logger = new RecordingLogger<Result<string?>>();
        var decorator = new LoggingInteractorDecorator<Input, Result<string?>>(
            new StubInteractor<Result<string?>>(result), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.IsType<ErrorCollection>(output.Value);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message == "Execução concluída.");
    }

    [Fact]
    public async Task Null_success_is_logged_as_completed()
    {
        var logger = new RecordingLogger<Result<string?>>();
        var decorator = new LoggingInteractorDecorator<Input, Result<string?>>(
            new StubInteractor<Result<string?>>(Result<string?>.Ok(null)), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.Null(Assert.IsType<Success<string?>>(output.Value).Value);
        Assert.Contains(logger.Entries, entry => entry.Message == "Execução concluída.");
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Uninitialized_result_is_rejected_instead_of_logged_as_success()
    {
        var logger = new RecordingLogger<Result<string?>>();
        var decorator = new LoggingInteractorDecorator<Input, Result<string?>>(
            new StubInteractor<Result<string?>>(default), logger);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await decorator.InteractAsync(new Input(), CancellationToken.None));

        Assert.DoesNotContain(logger.Entries, entry => entry.Message == "Execução concluída.");
    }

    [Fact]
    public async Task Custom_union_case_is_logged_as_completed_without_classifying_it_as_success()
    {
        var approval = new ApprovalRequired(20_000);
        CustomOutcome result = approval;
        var logger = new RecordingLogger<CustomOutcome>();
        var decorator = new LoggingInteractorDecorator<Input, CustomOutcome>(
            new StubInteractor<CustomOutcome>(result), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.Same(approval, Assert.IsType<ApprovalRequired>(output.Value));
        Assert.Contains(logger.Entries, entry => entry.Message == "Execução concluída.");
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Warning);
    }

    [Theory]
    [InlineData("plain output")]
    [InlineData(null)]
    public async Task Plain_output_is_returned_unchanged_and_logged_as_completed(string? result)
    {
        var logger = new RecordingLogger<string?>();
        var decorator = new LoggingInteractorDecorator<Input, string?>(
            new StubInteractor<string?>(result), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.Equal(result, output);
        Assert.Contains(logger.Entries, entry => entry.Message == "Execução concluída.");
    }

    [Fact]
    public async Task Single_error_output_is_logged_as_a_warning()
    {
        var error = Error.Invalid("business error");
        var logger = new RecordingLogger<Error>();
        var decorator = new LoggingInteractorDecorator<Input, Error>(
            new StubInteractor<Error>(error), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.Equal(error, output);
        Assert.Contains(logger.Entries, entry =>
            entry.Level == LogLevel.Warning && entry.Message.Contains(error.Message));
    }

    [Fact]
    public async Task Registration_decorates_custom_outputs_and_preserves_scoped_lifetime()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInteractor<Input, CustomOutcome, CustomInteractor>();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<IInteractor<Input, CustomOutcome>>();
        var second = secondScope.ServiceProvider.GetRequiredService<IInteractor<Input, CustomOutcome>>();
        Assert.IsType<LoggingInteractorDecorator<Input, CustomOutcome>>(first);
        Assert.Same(first, firstScope.ServiceProvider.GetRequiredService<IInteractor<Input, CustomOutcome>>());
        Assert.NotSame(first, second);

        var output = await first.InteractAsync(new Input(), CancellationToken.None);
        Assert.IsType<ApprovalRequired>(output.Value);
    }

    private sealed record Input : IMessage;

    private sealed record OrderCreated(Guid Id);

    private sealed record ApprovalRequired(decimal Total);

    private union CustomOutcome(OrderCreated, ApprovalRequired, ErrorCollection);

    private sealed class CustomInteractor : IInteractor<Input, CustomOutcome>
    {
        public CustomInteractor()
        {
        }

        public ValueTask<CustomOutcome> InteractAsync(Input input, CancellationToken cancellationToken)
            => ValueTask.FromResult<CustomOutcome>(new ApprovalRequired(20_000));
    }

    private sealed class StubInteractor<TOutput>(TOutput output) : IInteractor<Input, TOutput>
    {
        public ValueTask<TOutput> InteractAsync(Input input, CancellationToken cancellationToken)
            => ValueTask.FromResult(output);
    }

    private sealed class RecordingLogger<TOutput> : ILogger<LoggingInteractorDecorator<Input, TOutput>>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}
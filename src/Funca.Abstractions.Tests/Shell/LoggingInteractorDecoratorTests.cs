using Funca.Abstractions.Containers;
using Funca.Abstractions.Shell;
using Microsoft.Extensions.Logging;

namespace Funca.Abstractions.Tests.Shell;

public sealed class LoggingInteractorDecoratorTests
{
    [Fact]
    public async Task Union_failure_is_logged_as_a_warning()
    {
        var result = Result<string?>.Fail(Error.Invalid("failure"));
        var logger = new RecordingLogger();
        var decorator = new LoggingInteractorDecorator<Input, string?, Result<string?>>(
            new StubInteractor(result), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.IsType<ErrorCollection>(output.Value);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message == "Executado com sucesso.");
    }

    [Fact]
    public async Task Null_success_is_logged_as_success()
    {
        var logger = new RecordingLogger();
        var decorator = new LoggingInteractorDecorator<Input, string?, Result<string?>>(
            new StubInteractor(Result<string?>.Ok(null)), logger);

        var output = await decorator.InteractAsync(new Input(), CancellationToken.None);

        Assert.Null(Assert.IsType<Success<string?>>(output.Value).Value);
        Assert.Contains(logger.Entries, entry => entry.Message == "Executado com sucesso.");
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Uninitialized_result_is_rejected_instead_of_logged_as_success()
    {
        var logger = new RecordingLogger();
        var decorator = new LoggingInteractorDecorator<Input, string?, Result<string?>>(
            new StubInteractor(default), logger);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await decorator.InteractAsync(new Input(), CancellationToken.None));

        Assert.DoesNotContain(logger.Entries, entry => entry.Message == "Executado com sucesso.");
    }

    private sealed record Input : IMessage;

    private sealed class StubInteractor(Result<string?> output) : IInteractor<Input, string?, Result<string?>>
    {
        public ValueTask<Result<string?>> InteractAsync(Input input, CancellationToken cancellationToken)
            => ValueTask.FromResult(output);
    }

    private sealed class RecordingLogger : ILogger<LoggingInteractorDecorator<Input, string?, Result<string?>>>
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
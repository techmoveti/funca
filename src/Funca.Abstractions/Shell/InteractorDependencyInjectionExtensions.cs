namespace Funca.Abstractions.Shell;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

public static class InteractorDependencyInjectionExtensions
{
    public static IServiceCollection AddInteractor<TInput, TSuccess, TOutput, TImplementation>(
        this IServiceCollection services)
        where TInput : class, IMessage
        where TOutput : IOutcome<TSuccess>
        where TImplementation : class, IInteractor<TInput, TSuccess, TOutput>
    {
        services.AddScoped<TImplementation>();

        services.AddScoped<IInteractor<TInput, TSuccess, TOutput>>(provider =>
        {
            var innerHandler = provider.GetRequiredService<TImplementation>();

            return new LoggingInteractorDecorator<TInput, TSuccess, TOutput>(
                innerHandler,
                provider.GetRequiredService<ILogger<LoggingInteractorDecorator<TInput, TSuccess, TOutput>>>()
            );
        });

        return services;
    }
}

public sealed class LoggingInteractorDecorator<TInput, TSuccess, TOutput>(
    IInteractor<TInput, TSuccess, TOutput> inner,
    ILogger<LoggingInteractorDecorator<TInput, TSuccess, TOutput>> logger)
    : IInteractor<TInput, TSuccess, TOutput>
    where TInput : class, IMessage
    where TOutput : IOutcome<TSuccess>
{
    public async ValueTask<TOutput> InteractAsync(TInput input, CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando execução de {Name}", typeof(TInput).Name);

        var output = await inner.InteractAsync(input, cancellationToken);

        // O parâmetro genérico não recebe o desempacotamento automático de unions.
        object? value = output is IUnion union ? union.Value : output;

        switch (value)
        {
            case ErrorCollection:
                logger.LogWarning("Falha de validação acumulativa.");

                break;
            case Error er:
                logger.LogWarning("Erro de negócio: {Message}", er.Message);

                break;
            case null:
                throw new InvalidOperationException("O interactor retornou um resultado sem valor.");
            default:
                logger.LogInformation("Executado com sucesso.");

                break;
        }

        return output;
    }
}
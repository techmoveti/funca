namespace Funca.Abstractions.Shell;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

public static class InteractorDependencyInjectionExtensions
{
    public static IServiceCollection AddInteractor<TInput, TOutput, TImplementation>(
        this IServiceCollection services)
        where TInput : class, IMessage
        where TImplementation : class, IInteractor<TInput, TOutput>
    {
        services.AddScoped<TImplementation>();

        services.AddScoped<IInteractor<TInput, TOutput>>(provider =>
        {
            var innerHandler = provider.GetRequiredService<TImplementation>();

            return new LoggingInteractorDecorator<TInput, TOutput>(
                innerHandler,
                provider.GetRequiredService<ILogger<LoggingInteractorDecorator<TInput, TOutput>>>()
            );
        });

        return services;
    }
}

public sealed class LoggingInteractorDecorator<TInput, TOutput>(
    IInteractor<TInput, TOutput> inner,
    ILogger<LoggingInteractorDecorator<TInput, TOutput>> logger)
    : IInteractor<TInput, TOutput>
    where TInput : class, IMessage
{
    public async ValueTask<TOutput> InteractAsync(TInput input, CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando execução de {Name}", typeof(TInput).Name);

        var output = await inner.InteractAsync(input, cancellationToken);

        // O parâmetro genérico não recebe o desempacotamento automático de unions.
        object? value = output;
        if (value is IUnion union)
        {
            value = union.Value;

            if (value is null)
                throw new InvalidOperationException("O interactor retornou um resultado sem valor.");
        }

        switch (value)
        {
            case ErrorCollection:
                logger.LogWarning("Falha de validação acumulativa.");

                break;
            case Error er:
                logger.LogWarning("Erro de negócio: {Message}", er.Message);

                break;
            default:
                logger.LogInformation("Execução concluída.");

                break;
        }

        return output;
    }
}
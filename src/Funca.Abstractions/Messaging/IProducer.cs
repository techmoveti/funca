namespace Funca.Abstractions.Messaging;

public interface IProducer
{
    ValueTask ProduceAsync<T>(
        T message,
        CancellationToken cancellationToken) where T : IMessage;
}
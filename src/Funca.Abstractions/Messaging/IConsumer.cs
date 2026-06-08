namespace Funca.Abstractions.Messaging;

public interface IConsumer
{
    ValueTask ConsumeAsync<TMessage>(
        string queue,
        Func<TMessage, CancellationToken, ValueTask> handler,
        CancellationToken cancellationToken) where TMessage : IMessage;
}
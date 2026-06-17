namespace Funca.Abstractions.Data;

public interface IEvent : IMessage
{
    DateTimeOffset Timestamp { get; }
}
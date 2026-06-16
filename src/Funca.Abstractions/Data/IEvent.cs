namespace Funca.Abstractions.Data;

public interface IEvent : IMessage
{
    Guid AggregateId { get; }
    DateTimeOffset Timestamp { get; }
}
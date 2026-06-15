using System.Text.Json;

namespace Funca.Abstractions.Data;

/// <summary>
///     Event data store.
/// </summary>
public interface IEventStore
{
    ValueTask<EventEnvelopeState> AppendAsync(
        EventEnvelopeState envelope,
        CancellationToken cancellationToken);

    IAsyncEnumerable<EventEnvelopeState> LoadAsync(
        string aggregateType,
        Guid aggregateId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<EventEnvelopeState> LoadFromSequenceAsync(
        long sequence,
        CancellationToken cancellationToken);
}

/// <summary>
///     Event data.
/// </summary>
/// <param name="Sequence"></param>
/// <param name="Version"></param>
/// <param name="AggregateType"></param>
/// <param name="AggregateId"></param>
/// <param name="EventType"></param>
/// <param name="Timestamp"></param>
/// <param name="Payload"></param>
public record EventEnvelopeState(
    long Sequence,
    int Version,
    string AggregateType,
    Guid AggregateId,
    string EventType,
    DateTimeOffset Timestamp,
    JsonDocument Payload) : IState;
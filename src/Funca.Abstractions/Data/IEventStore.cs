namespace Funca.Abstractions.Data;

/// <summary>
///     Event data store - Imperative Shell for event sourcing.
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
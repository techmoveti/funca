namespace Funca.Abstractions.Data;

/// <summary>
///     Event data.
/// </summary>
/// <param name="Sequence"></param>
/// <param name="Version"></param>
/// <param name="TenantId"></param>
/// <param name="AggregateType"></param>
/// <param name="AggregateId"></param>
/// <param name="Timestamp"></param>
/// <param name="ActorId"></param>
/// <param name="ActorName"></param>
/// <param name="CorrelationId"></param>
/// <param name="EventType"></param>
/// <param name="Payload"></param>
public record EventEnvelopeState(
    long Sequence,
    int Version,
    TenantId TenantId,
    string AggregateType,
    Guid AggregateId,
    DateTimeOffset Timestamp,
    string? ActorId,
    string? ActorName,
    string? CorrelationId,
    string EventType,
    JsonDocument Payload) : IState, IRequireTenantPartition;
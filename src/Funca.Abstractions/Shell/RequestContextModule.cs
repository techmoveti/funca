using System.Text.Json;
using Funca.Abstractions.Data;

namespace Funca.Abstractions.Shell;

public static class RequestContextModule
{
    extension(RequestContext @this)
    {
        public void SetTenant(TenantId tenantId)
            => @this.Attach("tenantId", tenantId.Id);

        public Option<TenantId> GetTenant()
        {
            return @this
                .Detach<string>("tenantId")
                .Match(
                    s => Option<TenantId>.Some(new TenantId(s)),
                    Option.None<TenantId>);
        }

        public Result<EventEnvelopeState> WrapEvent<TEvent>(
            string aggregateType,
            int version,
            TEvent @event)
            where TEvent : IEvent
        {
            return Result.Ok(new EventEnvelopeState(
                Sequence: 0,
                version,
                AggregateType: aggregateType,
                AggregateId: @event.AggregateId,
                Timestamp: @event.Timestamp,
                ActorId: @this.UserContext.Value?.UserId,
                ActorName: @this.UserContext.Value?.UserName,
                CorrelationId: @this.CorrelationId.Value,
                EventType: @event.GetType().Name,
                Payload: JsonSerializer.SerializeToDocument(@event)));
        }
    }
}
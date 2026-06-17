using Funca.Abstractions.Data;

namespace Funca.Abstractions.Shell;

public static class RequestContextModule
{
    extension(RequestContext @this)
    {
        public void SetTenant(TenantId tenantId)
            => @this.Attach("tenantId", tenantId.Value);

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
            Guid aggregateId,
            int version,
            TEvent @event)
            where TEvent : IEvent
            => @this
                .GetTenant()
                .ToResult()
                .Map(tenantId =>
                    new EventEnvelopeState(
                        0,
                        version,
                        tenantId,
                        aggregateType,
                        aggregateId,
                        @event.Timestamp,
                        @this.UserContext.Value?.UserId,
                        @this.UserContext.Value?.UserName,
                        @this.CorrelationId.Value,
                        @event.GetType().Name,
                        JsonSerializer.SerializeToDocument(@event)));

        public Result<EventEnvelopeState> WrapEvent<TEvent, TAggregate>(
            Guid aggregateId,
            int version,
            TEvent @event)
            where TEvent : IEvent
            => @this.WrapEvent(typeof(TAggregate).Name, aggregateId, version, @event);
    }
}
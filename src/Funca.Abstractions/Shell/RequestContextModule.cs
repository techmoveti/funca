using Funca.Abstractions.Data;

namespace Funca.Abstractions.Shell;

public static class RequestContextModule
{
    extension(RequestContext @this)
    {
        public EventEnvelopeState WrapEvent<TEvent>(
            string aggregateType,
            Guid aggregateId,
            int version,
            TEvent @event) where TEvent : IEvent
            => new(
                0,
                version,
                @this.GetTenant(),
                aggregateType,
                aggregateId,
                @event.Timestamp,
                @this.UserContext?.UserId,
                @this.UserContext?.UserName,
                @this.CorrelationId,
                @event.GetType().Name,
                JsonSerializer.SerializeToElement(@event));

        public EventEnvelopeState WrapEvent<TEvent, TAggregate>(
            Guid aggregateId,
            int version,
            TEvent @event)
            where TEvent : IEvent
            => @this.WrapEvent(typeof(TAggregate).Name, aggregateId, version, @event);
    }
}
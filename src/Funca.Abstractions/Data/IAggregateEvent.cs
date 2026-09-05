namespace Funca.Abstractions.Data;

public interface IAggregateEvent<out TState> : IAggregate<TState> where TState : IState
{
    IEnumerable<IEvent> GetUncommittedEvents();
    void ClearUncommittedEvents();
    void Replay(IEnumerable<IEvent> events);
}
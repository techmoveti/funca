namespace Funca.Abstractions.Data;

public interface IAggregateEvent<TState> : IAggregate<TState> where TState : IState
{
    /// <summary>
    /// Try to get State snapshot.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    TState Snapshot { get; }

    IEnumerable<IEvent> GetUncommittedEvents();
    void ClearUncommittedEvents();
    void Replay(IEnumerable<IEvent> events);
}
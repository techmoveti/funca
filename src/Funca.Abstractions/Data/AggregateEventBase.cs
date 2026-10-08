namespace Funca.Abstractions.Data;

public abstract class AggregateEventBase<TState>(TState state) : IAggregateEvent<TState>
    where TState : IState
{
    private readonly List<IEvent> _uncommittedEvents = [];

    public TState State { get; protected set; } = state;
    public TState Snapshot => State;

    public IEnumerable<IEvent> GetUncommittedEvents()
        => [.. _uncommittedEvents];

    public void ClearUncommittedEvents()
        => _uncommittedEvents.Clear();

    public void Replay(IEnumerable<IEvent> events)
    {
        foreach (var @event in events)
            Apply(@event);
    }

    protected void Emit(IEvent @event)
    {
        Apply(@event);

        _uncommittedEvents.Add(@event);
    }

    protected abstract void Apply(IEvent @event);
}
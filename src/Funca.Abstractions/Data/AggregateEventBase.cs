namespace Funca.Abstractions.Data;

public abstract class AggregateEventBase<TState>(TState state) : IAggregateEvent<TState> where TState : IState
{
    public TState State { get; protected set; } = state;

    private readonly List<IEvent> _uncommittedEvents = [];

    protected void Emit(IEvent @event)
    {
        Apply(@event);

        _uncommittedEvents.Add(@event);
    }

    public IEnumerable<IEvent> GetUncommittedEvents()
        => [.. _uncommittedEvents];

    public void ClearUncommittedEvents()
        => _uncommittedEvents.Clear();

    public void Replay(IEnumerable<IEvent> events)
    {
        foreach (var @event in events)
            Apply(@event);
    }

    protected abstract void Apply(IEvent @event);
}
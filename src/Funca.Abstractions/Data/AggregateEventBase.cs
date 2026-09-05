namespace Funca.Abstractions.Data;

public abstract class AggregateEventBase<TState>(TState state) : IAggregateEvent<TState> where TState : IState
{
    public TState State { get; private set; } = state;

    private readonly List<IEvent> _uncommittedEvents = [];

    protected void Emit(IEvent @event)
    {
        State = Apply(State, @event);
        _uncommittedEvents.Add(@event);
    }

    public void Replay(IEnumerable<IEvent> events)
    {
        foreach (var @event in events)
            State = Apply(State, @event);
    }

    public IEnumerable<IEvent> GetUncommittedEvents()
        => [.. _uncommittedEvents];

    public void ClearUncommittedEvents()
        => _uncommittedEvents.Clear();

    protected abstract TState Apply(TState state, IEvent @event);
}
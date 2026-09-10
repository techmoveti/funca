namespace Funca.Abstractions.Data;

public abstract class AggregateEventBase<TState> : IAggregateEvent<TState> where TState : IState
{
    protected AggregateEventBase()
        => State = Option.None<TState>();

    protected AggregateEventBase(TState state)
        => State = Option.Some(state);

    public TState Snapshot =>
        State.Match(
            some => some,
            () => throw new InvalidOperationException("Aggregate has no state."));

    public Option<TState> State { get; protected set; }

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
namespace Funca.Abstractions.Data;

public interface IAggregate<TState> where TState : IState
{
    Option<TState> State { get; }
}
namespace Funca.Abstractions.Data;

public interface IAggregate<out TState> where TState : IState
{
    TState State { get; }
}
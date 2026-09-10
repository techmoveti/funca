namespace Funca.Abstractions.Data;

public interface IAggregate;

public interface IAggregate<out TState> : IAggregate where TState : IState
{
    TState State { get; }
}
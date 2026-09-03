namespace Funca.Abstractions.Data;

public interface IState;

public interface IState<out TKey> : IState where TKey : notnull
{
    TKey Id { get; }
}
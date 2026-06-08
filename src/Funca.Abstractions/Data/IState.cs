namespace Funca.Abstractions.Data;

/// <summary>
///     State interface.
/// </summary>
public interface IState;

/// <summary>
///     Specialized state interface with a key.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IState<out TKey> : IState where TKey : notnull
{
    TKey Id { get; }
}
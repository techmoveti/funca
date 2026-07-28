namespace Funca.Abstractions.Data;

public interface IState;

public interface IState<out TKey> : IState where TKey : notnull
{
    TKey Id { get; }
}

/// <summary>
///     State/Snapshot in Event Sourcing Approach
/// </summary>
public interface ISnapshotState
{
    int Version { get; }
    public DateTime SnapshotAt { get; set; }
}
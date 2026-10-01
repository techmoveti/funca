namespace Funca.Abstractions.Data;

/// <summary>
///     State/Snapshot in Event Sourcing Approach
/// </summary>
public interface IStateSnapshot
{
    /// <summary>The last event version incorporated into this snapshot.</summary>
    int Version { get; }

    DateTimeOffset SnapshotAt { get; }
}
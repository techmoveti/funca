namespace Funca.Abstractions.Data;

/// <summary>
///     State/Snapshot in Event Sourcing Approach
/// </summary>
public interface IStateSnapshot
{
    int Version { get; }
    public DateTime SnapshotAt { get; set; }
}
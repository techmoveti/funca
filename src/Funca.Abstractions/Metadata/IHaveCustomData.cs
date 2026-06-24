namespace Funca.Abstractions.Metadata;

/// <summary>
/// Defines an interface for objects that have a collection of custom data.
/// This interface is commonly used to represent entities or fields with
/// additional metadata or information encapsulated in the form of custom data entries.
/// </summary>
public interface IHaveCustomData
{
    IEnumerable<CustomData> CustomData { get; }
}

/// <summary>
/// Represents a piece of custom data associated with an entity or field.
/// This class is typically used to store additional metadata or information
/// that supplements the core properties of an entity.
/// </summary>
public sealed record CustomData
{
    public required FieldId Id { get; init; }
    public required string Value { get; init; }
}
using Funca.Abstractions.Data;

namespace Funca.Abstractions.Metadata;

public readonly record struct FieldId(string Value);

public readonly record struct FieldValueId(string Value);

public readonly record struct StateId(string Value);

/// <summary>
/// Represents the metadata for a custom field associated with a specific entity.
/// This class provides detailed information about a custom field, including its
/// identifier, name, type, and configuration properties such as required status and activation state.
/// </summary>
public sealed record CustomFieldMetadata : IRequireTenantPartition
{
    public required TenantId TenantId { get; init; }
    public required FieldId Id { get; init; }
    public required string EntityName { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required FieldType Type { get; init; }

    public string? Description { get; init; }
    public string? SelectOptions { get; init; }
    public string? DefaultValue { get; init; }
    public bool Required { get; init; }
    public bool Active { get; init; } = true;
}

/// <summary>
/// Represents the value of a custom field for a specific entity.
/// This class encapsulates the field value data, including its unique identifier,
/// associated field identifier, and the actual value stored.
/// </summary>
public sealed record CustomFieldValue
{
    public required TenantId TenantId { get; init; }
    public required string EntityName { get; init; }
    public required StateId StateId { get; init; }
    public required FieldValueId ValueId { get; init; }
    public required FieldId FieldId { get; init; }
    public required string Value { get; init; }
}
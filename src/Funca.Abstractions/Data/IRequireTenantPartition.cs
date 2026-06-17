namespace Funca.Abstractions.Data;

/// <summary>
///     Tenant Partition Abstraction - Represents a partitioned data context for multi-tenant applications.
/// </summary>
public interface IRequireTenantPartition
{
    TenantId TenantId { get; }
}

/// <summary>
///     TenantId - Represents a unique identifier for a tenant in a multi-tenant application.
/// </summary>
public readonly record struct TenantId(string Value);
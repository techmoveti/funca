using Funca.Abstractions.Data;

namespace Funca.Abstractions.Shell;

public sealed class RequestContext
{
    public string CorrelationId { get; private set; } = Guid.NewGuid().ToString();

    public RequestContext SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;

        return this;
    }

    public UserContext? UserContext { get; private set; }

    public RequestContext SetUserContext(UserContext userContext)
    {
        UserContext = userContext;

        return this;
    }

    public TenantId? TenantId { get; private set; }

    public void SetTenant(TenantId tenantId)
        => TenantId = tenantId;

    public TenantId GetTenant()
        => TenantId ?? throw new InvalidOperationException("Tenant must be set!");

    private readonly Lazy<Dictionary<string, object>> _attachments =
        new(() => new Dictionary<string, object>());

    public void Attach<T>(string accessKey, T valueOfT) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);
        ArgumentNullException.ThrowIfNull(valueOfT);

        _attachments.Value[accessKey] = valueOfT;
    }

    public T? Detach<T>(string accessKey) where T : class
    {
        if (!_attachments.IsValueCreated)
            return null;

        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);

        return _attachments.Value.TryGetValue(accessKey, out var content) && content is T typedContent
            ? typedContent
            : null;
    }
}
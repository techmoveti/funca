namespace Funca.Abstractions.Shell;

public sealed class RequestContext
{
    public Option<string> CorrelationId { get; private set; } = Option.None<string>();

    public RequestContext SetCorrelationId(string correlationId)
    {
        CorrelationId = Option.Some(correlationId);

        return this;
    }

    public Option<UserContext> UserContext { get; private set; } = Option.None<UserContext>();

    public RequestContext SetUserContext(UserContext userContext)
    {
        UserContext = Option.Some(userContext);

        return this;
    }

    private readonly Lazy<Dictionary<string, object>> _attachments =
        new(() => new Dictionary<string, object>());

    public void Attach<T>(string accessKey, T valueOfT) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);
        ArgumentNullException.ThrowIfNull(valueOfT);

        _attachments.Value[accessKey] = valueOfT;
    }

    public Option<T> Detach<T>(string accessKey) where T : class
    {
        if (!_attachments.IsValueCreated)
            return Option.None<T>();

        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);

        return _attachments.Value.TryGetValue(accessKey, out var content) && content is T typedContent
            ? Option.Some(typedContent)
            : Option.None<T>();
    }
}
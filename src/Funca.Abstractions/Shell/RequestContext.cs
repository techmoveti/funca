namespace Funca.Abstractions.Shell;

public sealed class RequestContext
{
    private readonly Lazy<Dictionary<string, object>> _attachments =
        new(() => new Dictionary<string, object>());

    public void Attach<T>(string accessKey, T valueOfT) where T : class
    {
        _attachments
            .Value
            .TryAdd(accessKey, valueOfT);
    }

    public Option<T> Detach<T>(string accessKey) where T : class
    {
        if (!_attachments.IsValueCreated)
            return Option.None<T>();

        return
            _attachments.Value.TryGetValue(accessKey, out var content)
                ? Option.Some((T)content)
                : Option.None<T>();
    }
}
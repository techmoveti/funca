using System.Runtime.CompilerServices;

namespace Funca.Abstractions.Data.EF;

public class EFEventStore<TDataContext> : IEventStore
    where TDataContext : DbContext, IDataContext
{
    protected readonly DbContext DataContext;
    private readonly TenantId? _tenantId;

    public EFEventStore(TDataContext dataContext)
        : this(dataContext, null)
    {
    }

    public EFEventStore(TDataContext dataContext, TenantId? tenantId)
    {
        ArgumentNullException.ThrowIfNull(dataContext);
        DataContext = dataContext;
        _tenantId = tenantId;
    }

    private IQueryable<EventEnvelopeState> Events => TenantQuery.Apply(
        DataContext.Set<EventEnvelopeState>().AsNoTracking(), _tenantId);

    public async ValueTask<EventEnvelopeState> AppendAsync(
        EventEnvelopeState envelope,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (_tenantId is { } tenant && envelope.TenantId != tenant)
            throw new ArgumentException("The event belongs to a different tenant.", nameof(envelope));

        var entry = await DataContext.Set<EventEnvelopeState>()
            .AddAsync(envelope, cancellationToken);

        await DataContext.SaveChangesAsync(cancellationToken);

        return entry.Entity;
    }

    public async IAsyncEnumerable<EventEnvelopeState> LoadAsync(
        string aggregateType,
        Guid aggregateId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = Events
            .Where(p => p.AggregateType == aggregateType && p.AggregateId == aggregateId)
            .OrderBy(p => p.Version)
            .ThenBy(p => p.Sequence);

        await foreach (var envelope in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            yield return envelope;
    }

    public async IAsyncEnumerable<EventEnvelopeState> LoadFromSequenceAsync(
        long sequence,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = Events
            .Where(p => p.Sequence >= sequence)
            .OrderBy(p => p.Sequence);

        await foreach (var envelope in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            yield return envelope;
    }
}
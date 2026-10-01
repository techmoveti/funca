using System.Runtime.CompilerServices;

namespace Funca.Abstractions.Data.EF;

public class EFEventStore<TDataContext>(TDataContext dataContext) : IEventStore
    where TDataContext : DbContext, IDataContext
{
    protected readonly DbContext DataContext = dataContext;

    private IQueryable<EventEnvelopeState> Events
        => DataContext.Set<EventEnvelopeState>().AsNoTracking();

    public async ValueTask<EventEnvelopeState> AppendAsync(
        EventEnvelopeState envelope,
        CancellationToken cancellationToken)
    {
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
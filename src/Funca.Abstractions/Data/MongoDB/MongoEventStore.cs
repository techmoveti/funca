using System.Runtime.CompilerServices;
using MongoDB.Driver;

namespace Funca.Abstractions.Data.MongoDB;

public class MongoEventStore<TConnection> : IEventStore
    where TConnection : MongoConnectionWrapper
{
    protected readonly IMongoCollection<EventEnvelopeState> Collection;
    private readonly IClientSessionHandle? _session;
    private readonly TenantId? _tenantId;

    public MongoEventStore(
        TConnection connection,
        string collectionName = "events",
        IClientSessionHandle? session = null)
        : this(connection, collectionName, session, null)
    {
    }

    public MongoEventStore(
        TConnection connection,
        string collectionName,
        IClientSessionHandle? session,
        TenantId? tenantId)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        MongoEventSerialization.Configure();
        Collection = connection.Database.GetCollection<EventEnvelopeState>(collectionName);
        _session = session;
        _tenantId = tenantId;
    }

    /// <summary>
    /// Appends the supplied envelope without generating its Sequence or Version.
    /// When a session is supplied, the caller owns the transaction and its commit.
    /// </summary>
    public async ValueTask<EventEnvelopeState> AppendAsync(
        EventEnvelopeState envelope,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (_tenantId is { } tenant && envelope.TenantId != tenant)
            throw new ArgumentException("The event belongs to a different tenant.", nameof(envelope));

        if (_session is null)
            await Collection.InsertOneAsync(envelope, cancellationToken: cancellationToken);
        else
            await Collection.InsertOneAsync(_session, envelope, cancellationToken: cancellationToken);

        return envelope;
    }

    public IAsyncEnumerable<EventEnvelopeState> LoadAsync(
        string aggregateType,
        Guid aggregateId,
        CancellationToken cancellationToken)
    {
        var filter = Builders<EventEnvelopeState>.Filter.And(
            Builders<EventEnvelopeState>.Filter.Eq(p => p.AggregateType, aggregateType),
            Builders<EventEnvelopeState>.Filter.Eq(p => p.AggregateId, aggregateId));

        var sort = Builders<EventEnvelopeState>.Sort
            .Ascending(p => p.Version)
            .Ascending(p => p.Sequence);

        return ReadAsync(ApplyTenant(filter), sort, cancellationToken);
    }

    public IAsyncEnumerable<EventEnvelopeState> LoadFromSequenceAsync(
        long sequence,
        CancellationToken cancellationToken)
    {
        var filter = Builders<EventEnvelopeState>.Filter.Gte(p => p.Sequence, sequence);
        var sort = Builders<EventEnvelopeState>.Sort.Ascending(p => p.Sequence);

        return ReadAsync(ApplyTenant(filter), sort, cancellationToken);
    }

    private FilterDefinition<EventEnvelopeState> ApplyTenant(FilterDefinition<EventEnvelopeState> filter)
        => _tenantId is { } tenant
            ? filter & Builders<EventEnvelopeState>.Filter.Eq(p => p.TenantId, tenant)
            : filter;

    /// <summary>
    /// Creates indexes during application initialization, outside a transaction.
    /// Enable uniqueness only when an aggregate version identifies a single event.
    /// </summary>
    public Task EnsureIndexesAsync(
        bool uniqueAggregateVersion = true,
        CancellationToken cancellationToken = default)
    {
        var aggregate = new CreateIndexModel<EventEnvelopeState>(
            Builders<EventEnvelopeState>.IndexKeys
                .Ascending(p => p.TenantId)
                .Ascending(p => p.AggregateType)
                .Ascending(p => p.AggregateId)
                .Ascending(p => p.Version),
            new CreateIndexOptions { Unique = uniqueAggregateVersion });

        // Supports LoadAsync, whose contract does not include a tenant filter.
        var aggregateRead = new CreateIndexModel<EventEnvelopeState>(
            Builders<EventEnvelopeState>.IndexKeys
                .Ascending(p => p.AggregateType)
                .Ascending(p => p.AggregateId)
                .Ascending(p => p.Version)
                .Ascending(p => p.Sequence));

        var sequence = new CreateIndexModel<EventEnvelopeState>(
            Builders<EventEnvelopeState>.IndexKeys.Ascending(p => p.Sequence));

        return Collection.Indexes.CreateManyAsync(
            [aggregate, aggregateRead, sequence], cancellationToken);
    }

    private async IAsyncEnumerable<EventEnvelopeState> ReadAsync(
        FilterDefinition<EventEnvelopeState> filter,
        SortDefinition<EventEnvelopeState> sort,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = _session is null
            ? Collection.Find(filter)
            : Collection.Find(_session, filter);

        using var cursor = await query.Sort(sort).ToCursorAsync(cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
            foreach (var envelope in cursor.Current)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return envelope;
            }
    }
}
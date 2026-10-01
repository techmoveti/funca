using MongoDB.Driver;
using MongoQueryable = MongoDB.Driver.Linq.MongoQueryable;

namespace Funca.Abstractions.Data.MongoDB;

public class MongoQueryStore<TConnection, TState, TKey> : IQueryStore<TState, TKey>
    where TConnection : MongoConnectionWrapper
    where TState : class, IState<TKey>
    where TKey : notnull
{
    protected readonly IMongoCollection<TState> Collection;
    private readonly IClientSessionHandle? _session;
    private readonly TenantId? _tenantId;

    protected MongoQueryStore(TConnection connection, string collectionName)
        : this(connection, collectionName, null)
    {
    }

    protected MongoQueryStore(
        TConnection connection,
        string collectionName,
        IClientSessionHandle? session,
        TenantId? tenantId = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        Collection = connection.Database.GetCollection<TState>(collectionName);
        _session = session;
        _tenantId = tenantId;
    }

    private IQueryable<TState> States => TenantQuery.Apply(
        _session is null ? Collection.AsQueryable() : Collection.AsQueryable(_session), _tenantId);

    public async Task<Option<TState>> GetAsync(TKey id, CancellationToken token)
    {
        var state = await MongoQueryable.FirstOrDefaultAsync(
            States.Where(p => p.Id.Equals(id)), token);

        return state is null
            ? Option<TState>.None()
            : Option<TState>.Some(state);
    }

    public async Task<Option<TModel>> GetProjectedAsync<TModel>(
        TKey id,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        // Preserve the distinction between a missing document and a projection
        // whose value is default(TModel), including value types such as int.
        var models = await MongoQueryable.ToListAsync(
            States
                .Where(p => p.Id.Equals(id))
                .Take(1)
                .Select(projection),
            token);

        return models.Count == 0 || models[0] is null
            ? Option<TModel>.None()
            : Option<TModel>.Some(models[0]);
    }

    public async Task<IReadOnlyList<TState>> GetManyAsync(
        IReadOnlyCollection<TKey> ids,
        CancellationToken token)
    {
        return await MongoQueryable.ToListAsync(
            States.Where(p => ids.Contains(p.Id)), token);
    }

    public async Task<IEnumerable<TModel>> GetManyProjectedAsync<TModel>(
        IReadOnlyCollection<TKey> ids,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        return await MongoQueryable.ToListAsync(
            States
                .Where(p => ids.Contains(p.Id))
                .Select(projection),
            token);
    }

    public async Task<QueryResult<IReadOnlyList<TState>>> GetManyAsync(
        Query<TState, TKey> query,
        CancellationToken token)
    {
        var skip = query.Skip();
        var filter = query.Apply(States);
        var count = await MongoQueryable.LongCountAsync(filter, token);
        var data = await MongoQueryable.ToListAsync(
            query.ApplyOrdering(filter).Skip(skip).Take(query.PageSize), token);

        return new QueryResult<IReadOnlyList<TState>>(
            data, query.Page, query.PageSize, count);
    }

    public async Task<QueryResult<IReadOnlyList<TModel>>> GetManyProjectedAsync<TModel>(
        Query<TState, TKey> query,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        var skip = query.Skip();
        var filter = query.Apply(States);
        var count = await MongoQueryable.LongCountAsync(filter, token);
        var data = await MongoQueryable.ToListAsync(
            query.ApplyOrdering(filter).Skip(skip).Take(query.PageSize).Select(projection), token);

        return new QueryResult<IReadOnlyList<TModel>>(
            data, query.Page, query.PageSize, count);
    }
}
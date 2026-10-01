namespace Funca.Abstractions.Data.EF;

public class EFQueryStore<TDataContext, TState, TKey> : IQueryStore<TState, TKey>
    where TDataContext : DbContext, IDataContext
    where TState : class, IState<TKey>
    where TKey : notnull
{
    protected readonly DbContext DataContext;
    private readonly TenantId? _tenantId;

    protected EFQueryStore(TDataContext dataContext)
        : this(dataContext, null)
    {
    }

    protected EFQueryStore(TDataContext dataContext, TenantId? tenantId)
    {
        ArgumentNullException.ThrowIfNull(dataContext);
        DataContext = dataContext;
        _tenantId = tenantId;
    }

    private IQueryable<TState> States => TenantQuery.Apply(
        DataContext.Set<TState>().AsNoTracking(), _tenantId);

    public async Task<Option<TState>> GetAsync(TKey id, CancellationToken token)
    {
        var state = await States.Where(p => p.Id.Equals(id)).FirstOrDefaultAsync(token);

        return state is null
            ? Option<TState>.None()
            : Option<TState>.Some(state);
    }

    public async Task<Option<TModel>> GetProjectedAsync<TModel>(
        TKey id,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        var models = await States
            .Where(p => p.Id!.Equals(id))
            .Take(1)
            .Select(projection)
            .ToListAsync(token);

        return models.Count == 0 || models[0] is null
            ? Option<TModel>.None()
            : Option<TModel>.Some(models[0]);
    }

    public async Task<IReadOnlyList<TState>> GetManyAsync(IReadOnlyCollection<TKey> ids, CancellationToken token)
    {
        return await States
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(token);
    }

    public async Task<IEnumerable<TModel>> GetManyProjectedAsync<TModel>(
        IReadOnlyCollection<TKey> ids,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        return await States
            .Where(p => ids.Contains(p.Id))
            .Select(projection)
            .ToListAsync(token);
    }

    public async Task<QueryResult<IReadOnlyList<TState>>> GetManyAsync(
        Query<TState, TKey> query,
        CancellationToken token)
    {
        var skip = query.Skip();
        var filter = query.Apply(States);
        var count = await filter.LongCountAsync(token);
        var data = await query.ApplyOrdering(filter)
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(token);

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
        var count = await filter.LongCountAsync(token);
        var data = await query.ApplyOrdering(filter)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(projection)
            .ToListAsync(token);

        return new QueryResult<IReadOnlyList<TModel>>(
            data, query.Page, query.PageSize, count);
    }
}
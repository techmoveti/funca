namespace Funca.Abstractions.Data.EF;

public class EFQueryStore<TDataContext, TState, TKey> : IQueryStore<TState, TKey>
    where TDataContext : DbContext, IDataContext
    where TState : class, IState<TKey>
    where TKey : notnull
{
    protected readonly DbContext DataContext;

    protected EFQueryStore(TDataContext dataContext)
    {
        if (dataContext is DbContext specializedContext)
            DataContext = specializedContext;
        else
            throw new ArgumentException("Invalid Data Context");
    }

    public async Task<Option<TState>> GetAsync(TKey id, CancellationToken token)
    {
        var state = await DataContext.FindAsync<TState>([id], token);

        return state is null
            ? Option<TState>.None()
            : Option<TState>.Some(state);
    }

    public async Task<Option<TModel>> GetProjectedAsync<TModel>(
        TKey id,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        var model = await DataContext.Set<TState>()
            .Where(p => p.Id!.Equals(id))
            .Select(projection)
            .FirstOrDefaultAsync(token);

        return model is null
            ? Option<TModel>.None()
            : Option<TModel>.Some(model);
    }

    public async Task<IReadOnlyList<TState>> GetManyAsync(IReadOnlyCollection<TKey> ids, CancellationToken token)
    {
        return await DataContext.Set<TState>()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(token);
    }

    public async Task<IEnumerable<TModel>> GetManyProjectedAsync<TModel>(
        IReadOnlyCollection<TKey> ids,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token)
    {
        return await DataContext.Set<TState>()
            .Where(p => ids.Contains(p.Id))
            .Select(projection)
            .ToListAsync(token);
    }

    public async Task<QueryResult<IReadOnlyList<TState>>> GetManyAsync(
        Query<TState, TKey> query,
        CancellationToken token)
    {
        var filter = query.Apply(DataContext.Set<TState>());
        var count = await filter.LongCountAsync(token);
        var data = await filter
            .Skip(query.Skip())
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
        var filter = query.Apply(DataContext.Set<TState>());
        var count = await filter.LongCountAsync(token);
        var data = await filter
            .Skip(query.Skip())
            .Take(query.PageSize)
            .Select(projection)
            .ToListAsync(token);

        return new QueryResult<IReadOnlyList<TModel>>(
            data, query.Page, query.PageSize, count);
    }
}
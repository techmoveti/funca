namespace Funca.Abstractions.Data;

/// <summary>
///     Query Store Abstraction - Imperative Shell for querying state.
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IQueryStore<TState, TKey>
    where TState : class, IState<TKey> where TKey : notnull
{
    Task<Option<TState>> GetAsync(TKey id, CancellationToken token);

    Task<Option<TModel>> GetProjectedAsync<TModel>(
        TKey id,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token);

    Task<IReadOnlyList<TState>> GetManyAsync(IReadOnlyCollection<TKey> ids, CancellationToken token);

    Task<IEnumerable<TModel>> GetManyProjectedAsync<TModel>(
        IReadOnlyCollection<TKey> ids,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token);

    Task<QueryResult<IReadOnlyList<TState>>> GetManyAsync(
        Query<TState, TKey> query,
        CancellationToken token);

    Task<QueryResult<IReadOnlyList<TModel>>> GetManyProjectedAsync<TModel>(
        Query<TState, TKey> query,
        Expression<Func<TState, TModel>> projection,
        CancellationToken token);
}
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
        Func<TState, TModel> projection,
        CancellationToken token);

    Task<IEnumerable<TState>> GetManyAsync(TKey[] ids, CancellationToken token);

    Task<IEnumerable<TModel>> GetManyProjectedAsync<TModel>(
        TKey[] ids,
        Func<TState, TModel> projection,
        CancellationToken token);

    Task<QueryResult<IEnumerable<TState>>> GetManyAsync(
        Query<TState, TKey> query,
        CancellationToken token);

    Task<QueryResult<IEnumerable<TModel>>> GetManyProjectedAsync<TModel>(
        Query<TState, TKey> query,
        Func<TState, TModel> projection,
        CancellationToken token);
}
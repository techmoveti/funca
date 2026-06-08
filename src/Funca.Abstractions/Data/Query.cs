using System.ComponentModel;

namespace Funca.Abstractions.Data;

/// <summary>
///     State Store Abstraction - Imperative Shell for managing state.
/// </summary>
public abstract record Query
{
    [DefaultValue(1)] public int Page { get; init; } = 1;

    [DefaultValue(10)] public int PageSize { get; init; } = 10;

    public string? SortBy { get; init; }
    public QueryOrder? OrderType { get; init; } = QueryOrder.None;

    public int Skip() => PageSize * (Page - 1);
}

public abstract record Query<TState, TKey> : Query
    where TState : class, IState<TKey> where TKey : notnull
{
    public virtual IQueryable<TState> Apply(IQueryable<TState> query) => query;
}
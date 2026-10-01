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

    public int Skip()
    {
        if (Page < 1)
            throw new ArgumentOutOfRangeException(nameof(Page), Page, "Page must be greater than zero.");
        if (PageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(PageSize), PageSize, "PageSize must be greater than zero.");

        return checked(PageSize * (Page - 1));
    }
}

public abstract record Query<TState, TKey> : Query
    where TState : class, IState<TKey> where TKey : notnull
{
    public virtual IQueryable<TState> Apply(IQueryable<TState> query) => query;

    /// <summary>Applies requested ordering and a stable ID tie-breaker before pagination.</summary>
    public IQueryable<TState> ApplyOrdering(IQueryable<TState> query)
    {
        if (!string.IsNullOrWhiteSpace(SortBy))
        {
            var parameter = Expression.Parameter(typeof(TState), "state");
            Expression member = parameter;
            foreach (var part in SortBy.Split('.'))
                member = Expression.PropertyOrField(member, part);

            var selector = Expression.Lambda(member, parameter);
            var method = OrderType == QueryOrder.Descending
                ? nameof(Queryable.OrderByDescending)
                : nameof(Queryable.OrderBy);

            query = query.Provider.CreateQuery<TState>(Expression.Call(
                typeof(Queryable), method, [typeof(TState), member.Type],
                query.Expression, Expression.Quote(selector)));
        }

        var ordered = AddTieBreaker(query.Expression);

        return ordered is null ? query.OrderBy(p => p.Id) : query.Provider.CreateQuery<TState>(ordered);
    }

    static private Expression? AddTieBreaker(Expression expression)
    {
        if (expression is not MethodCallExpression call || call.Arguments.Count == 0)
            return null;

        if (call.Method.DeclaringType == typeof(Queryable) &&
            call.Method.Name is nameof(Queryable.OrderBy) or nameof(Queryable.OrderByDescending)
                or nameof(Queryable.ThenBy) or nameof(Queryable.ThenByDescending))
        {
            Expression<Func<TState, TKey>> id = p => p.Id;

            return Expression.Call(typeof(Queryable), nameof(Queryable.ThenBy),
                [typeof(TState), typeof(TKey)], call, Expression.Quote(id));
        }

        // Insert ThenBy at the ordering node, before later Where calls whose
        // expression type is IQueryable rather than IOrderedQueryable.
        var source = AddTieBreaker(call.Arguments[0]);

        if (source is null)
            return null;

        var arguments = call.Arguments.ToArray();
        arguments[0] = source;

        return call.Update(call.Object, arguments);
    }
}
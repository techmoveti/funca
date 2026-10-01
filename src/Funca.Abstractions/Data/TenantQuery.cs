namespace Funca.Abstractions.Data;

static internal class TenantQuery
{
    public static IQueryable<TState> Apply<TState>(IQueryable<TState> query, TenantId? tenantId)
    {
        if (tenantId is not { } tenant)
            return query;

        if (!typeof(IRequireTenantPartition).IsAssignableFrom(typeof(TState)))
            throw new InvalidOperationException($"{typeof(TState).Name} does not support tenant partitioning.");

        var parameter = Expression.Parameter(typeof(TState), "state");
        var property = Expression.Property(parameter, nameof(IRequireTenantPartition.TenantId));
        var predicate = Expression.Lambda<Func<TState, bool>>(
            Expression.Equal(property, Expression.Constant(tenant)), parameter);

        return query.Where(predicate);
    }
}
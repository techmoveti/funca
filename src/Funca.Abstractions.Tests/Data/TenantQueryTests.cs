using Funca.Abstractions.Data;

namespace Funca.Abstractions.Tests.Data;

public sealed class TenantQueryTests
{
    [Fact]
    public void Tenant_filter_is_applied_before_id_lookup()
    {
        var states = new[]
        {
            new State(1, new TenantId("a")),
            new State(1, new TenantId("b"))
        }.AsQueryable();

        var result = TenantQuery.Apply(states, new TenantId("a")).Where(p => p.Id == 1);

        Assert.Equal("a", Assert.Single(result).TenantId.Value);
    }

    [Fact]
    public void Dedicated_database_mode_does_not_add_a_tenant_filter()
    {
        var states = new[] { new State(1, new TenantId("a")), new State(2, new TenantId("b")) }.AsQueryable();

        Assert.Equal(2, TenantQuery.Apply(states, null).Count());
    }

    [Fact]
    public void Tenant_scope_rejects_states_without_partition_contract()
    {
        Assert.Throws<InvalidOperationException>(() =>
            TenantQuery.Apply(new[] { "state" }.AsQueryable(), new TenantId("a")));
    }

    private sealed record State(int Id, TenantId TenantId) : IState<int>, IRequireTenantPartition;
}
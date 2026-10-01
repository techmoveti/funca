using Funca.Abstractions.Data;

namespace Funca.Abstractions.Tests.Data;

public sealed class QueryOrderingTests
{
    static private readonly IQueryable<State> States = new[]
    {
        new State(3, "B"), new State(2, "A"), new State(1, "A")
    }.AsQueryable();

    [Fact]
    public void Pagination_defaults_to_id_order()
    {
        var query = new TestQuery { Page = 2, PageSize = 1 };
        var result = query.ApplyOrdering(query.Apply(States)).Skip(query.Skip()).Take(query.PageSize);

        Assert.Equal(2, Assert.Single(result).Id);
    }

    [Fact]
    public void Requested_descending_sort_uses_id_to_break_ties()
    {
        var query = new TestQuery { SortBy = "Name", OrderType = QueryOrder.Descending };

        Assert.Equal(new[] { 3, 1, 2 }, query.ApplyOrdering(States).Select(p => p.Id));
    }

    [Fact]
    public void Custom_order_is_preserved_when_sort_is_not_requested()
    {
        var query = new OrderedQuery();

        Assert.Equal(new[] { 3, 1, 2 }, query.ApplyOrdering(query.Apply(States)).Select(p => p.Id));
    }

    [Fact]
    public void Requested_sort_overrides_custom_order()
    {
        var query = new OrderedQuery { SortBy = "Name", OrderType = QueryOrder.Ascending };

        Assert.Equal(new[] { 1, 2, 3 }, query.ApplyOrdering(query.Apply(States)).Select(p => p.Id));
    }

    private sealed record State(int Id, string Name) : IState<int>;

    private sealed record TestQuery : Query<State, int>;

    private sealed record OrderedQuery : Query<State, int>
    {
        public override IQueryable<State> Apply(IQueryable<State> query)
            => query.OrderByDescending(p => p.Name).Where(p => p.Id > 0);
    }
}
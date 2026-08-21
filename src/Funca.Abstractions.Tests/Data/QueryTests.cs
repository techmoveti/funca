using Funca.Abstractions.Data;

namespace Funca.Abstractions.Tests.Data;

public sealed class QueryTests
{
    [Fact]
    public void Skip_validates_pagination_inputs_and_checks_overflow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TestQuery { Page = 0 }.Skip());
        Assert.Throws<ArgumentOutOfRangeException>(() => new TestQuery { PageSize = 0 }.Skip());
        Assert.Throws<OverflowException>(() => new TestQuery { Page = int.MaxValue, PageSize = int.MaxValue }.Skip());
    }

    [Fact]
    public void PageCount_rejects_an_invalid_page_size()
    {
        var result = new QueryResult<string>("data", 1, 0, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = result.PageCount);
    }

    private sealed record TestQuery : Query;
}

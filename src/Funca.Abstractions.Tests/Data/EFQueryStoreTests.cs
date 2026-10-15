using Funca.Abstractions.Data;
using Funca.Abstractions.Data.EF;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Funca.Abstractions.Tests.Data;

public sealed class EFQueryStoreTests
{
    [Fact]
    public async Task Default_source_reads_mapped_states_without_tracking()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await Seed(db);
        var store = new EFQueryStore<TestContext, Row, int>(db);

        Assert.Equal("Beta", (await store.GetAsync(1, default))!.Name);
        Assert.Null(await store.GetAsync(99, default));
        Assert.Equal(3, (await store.GetManyAsync(new RowQuery(), default)).RecordCount);
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task All_operations_use_the_overridden_unmapped_projection_and_its_tenant_filter()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await Seed(db);
        var store = new ProjectionStore(db, "tenant-a");

        Assert.Null(db.Model.FindEntityType(typeof(Projection)));
        Assert.Equal("Beta", (await store.GetAsync(1, default))!.Name);
        Assert.Null(await store.GetAsync(3, default));
        Assert.Equal("Beta", await store.GetProjectedAsync(1, s => s.Name, default));
        Assert.Null(await store.GetProjectedAsync(3, s => s.Name, default));
        Assert.Equal(new[] { 1, 2 }, (await store.GetManyAsync([1, 2, 3], default)).Select(s => s.Id).Order());
        Assert.Equal(new[] { "Alpha", "Beta" },
            (await store.GetManyProjectedAsync([1, 2, 3], s => s.Name, default)).Order());

        var query = new ProjectionQuery { SortBy = "Name", PageSize = 1 };
        var first = await store.GetManyAsync(query, default);
        Assert.Equal(2, first.RecordCount);
        Assert.Equal(2, Assert.Single(first.Data!).Id);
        var second = await store.GetManyProjectedAsync(query with { Page = 2 }, s => s.Name, default);
        Assert.Equal(2, second.RecordCount);
        Assert.Equal(2, second.Page);
        Assert.Equal("Beta", Assert.Single(second.Data!));
        var filtered = await store.GetManyAsync(query with { Prefix = "B" }, default);
        Assert.Equal(1, filtered.RecordCount);
        Assert.Equal("Beta", Assert.Single(filtered.Data!).Name);
        Assert.Empty(db.ChangeTracker.Entries());

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.GetAsync(1, cancelled.Token));
    }

    private static TestContext CreateContext(SqliteConnection connection)
        => new(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);

    private static async Task Seed(TestContext db)
    {
        await db.Database.EnsureCreatedAsync();
        db.Set<Row>().AddRange(new Row(1, "tenant-a", "Beta"), new Row(2, "tenant-a", "Alpha"),
            new Row(3, "tenant-b", "Private"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : EFDbContextWrapper(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>().HasKey(s => s.Id);
    }

    private sealed record Row(int Id, string Tenant, string Name) : IState<int>;
    private sealed record Projection : IState<int>
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
    }
    private sealed record RowQuery : Query<Row, int>;

    private sealed record ProjectionQuery : Query<Projection, int>
    {
        public string? Prefix { get; init; }
        public override IQueryable<Projection> Apply(IQueryable<Projection> query)
            => Prefix is null ? query : query.Where(s => s.Name.StartsWith(Prefix));
    }

    private sealed class ProjectionStore(TestContext db, string tenant) : EFQueryStore<TestContext, Projection, int>(db)
    {
        protected override IQueryable<Projection> States
            => DataContext.Set<Row>().AsNoTracking().Where(s => s.Tenant == tenant)
                .Select(s => new Projection { Id = s.Id, Name = s.Name });
    }
}
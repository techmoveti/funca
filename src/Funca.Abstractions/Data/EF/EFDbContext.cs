namespace Funca.Abstractions.Data.EF;

public abstract class EFDbContext : DbContext, IDataContext
{
    protected EFDbContext()
    {
    }

    protected EFDbContext(DbContextOptions options) : base(options)
    {
    }
}
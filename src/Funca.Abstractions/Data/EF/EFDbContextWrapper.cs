namespace Funca.Abstractions.Data.EF;

public abstract class EFDbContextWrapper : DbContext, IDataContext
{
    protected EFDbContextWrapper()
    {
    }

    protected EFDbContextWrapper(DbContextOptions options) : base(options)
    {
    }
}
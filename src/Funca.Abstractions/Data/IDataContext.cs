namespace Funca.Abstractions.Data;

public interface IDataContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
namespace Funca.Abstractions.Data.EF;

public abstract class EFDbContextWrapper : DbContext, IDataContext
{
    protected EFDbContextWrapper()
    {
    }

    protected EFDbContextWrapper(DbContextOptions options) : base(options)
    {
    }

    public async Task ExecuteTransactionAsync(
        TransactionCallBack callback,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (Database.CurrentTransaction is not null)
            throw new InvalidOperationException(
                "Já existe uma transação ativa neste contexto.");

        await using var transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        await callback(cancellationToken);

        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
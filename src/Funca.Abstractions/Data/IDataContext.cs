namespace Funca.Abstractions.Data;

public delegate Task TransactionCallBack(CancellationToken cancellationToken);

public interface IDataContext
{
    Task ExecuteTransactionAsync(
        TransactionCallBack callback,
        CancellationToken cancellationToken = default);
}
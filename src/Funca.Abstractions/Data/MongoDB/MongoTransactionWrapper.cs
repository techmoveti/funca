using MongoDB.Driver;

namespace Funca.Abstractions.Data.MongoDB;

public sealed class MongoTransactionWrapper<TConnection>
    where TConnection : MongoConnectionWrapper
{
    private readonly TConnection _connection;

    public MongoTransactionWrapper(TConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
    }

    /// <summary>
    /// Executes sequential MongoDB operations in a transaction. The callback may
    /// be retried and must use the supplied session for every participating operation.
    /// </summary>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<IClientSessionHandle, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var session = await _connection.Client.StartSessionAsync(
            cancellationToken: cancellationToken);

        var options = new TransactionOptions(
            ReadConcern.Snapshot,
            ReadPreference.Primary,
            WriteConcern.WMajority);

        return await session.WithTransactionAsync(
            action,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Executes a transaction without returning a result, using the same retry behavior.
    /// </summary>
    public Task ExecuteInTransactionAsync(
        Func<IClientSessionHandle, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        return ExecuteInTransactionAsync(
            async (session, ct) =>
            {
                await action(session, ct);

                return true;
            },
            cancellationToken);
    }
}
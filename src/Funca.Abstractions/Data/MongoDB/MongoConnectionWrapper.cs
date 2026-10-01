using MongoDB.Driver;

namespace Funca.Abstractions.Data.MongoDB;

public abstract class MongoConnectionWrapper : IDisposable
{
    protected MongoConnectionWrapper(
        string connectionString,
        string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(databaseName);
    }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
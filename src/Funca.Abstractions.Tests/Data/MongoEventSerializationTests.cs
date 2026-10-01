using System.Text.Json;
using Funca.Abstractions.Data;
using Funca.Abstractions.Data.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Funca.Abstractions.Tests.Data;

public sealed class MongoEventSerializationTests
{
    [Theory]
    [InlineData("{\"nested\":{\"value\":1234567890123456789},\"items\":[true,null,\"ação\"]}")]
    [InlineData("[1,2,3]")]
    [InlineData("null")]
    [InlineData("false")]
    [InlineData("\"texto\"")]
    public void Envelope_round_trips_with_mongodb_id_and_preserves_payload(string json)
    {
        MongoEventSerialization.Configure();
        // Repeated registration is safe for multiple stores and connection types.
        MongoEventSerialization.Configure();

        using var payload = JsonDocument.Parse(json);
        var envelope = new EventEnvelopeState(
            42, 3, new TenantId("tenant-a"), "Pedido", Guid.NewGuid(),
            new DateTimeOffset(2026, 9, 30, 12, 34, 56, TimeSpan.FromHours(-3)),
            null, "Usuário", "correlation", "PedidoCriado", payload.RootElement.Clone());

        var document = envelope.ToBsonDocument();
        document["_id"] = ObjectId.GenerateNewId();

        var restored = BsonSerializer.Deserialize<EventEnvelopeState>(document);

        Assert.Equal(envelope with { Payload = restored.Payload }, restored);
        Assert.Equal(envelope.Timestamp.Offset, restored.Timestamp.Offset);
        Assert.Equal(json, restored.Payload.GetRawText());
        Assert.Equal(BsonType.String, document["TenantId"].BsonType);
        Assert.Equal(BsonType.String, document["Payload"].BsonType);
        Assert.Equal(BsonBinarySubType.UuidStandard, document["AggregateId"].AsBsonBinaryData.SubType);
    }
}
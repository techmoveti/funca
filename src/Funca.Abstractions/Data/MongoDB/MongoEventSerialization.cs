using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Funca.Abstractions.Data.MongoDB;

public static class MongoEventSerialization
{
    /// <summary>
    /// Registers the envelope mapping before its first use. An existing application
    /// mapping is preserved. Payload is stored as JSON text and TenantId as a string.
    /// </summary>
    public static void Configure()
    {
        BsonClassMap.TryRegisterClassMap<EventEnvelopeState>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.MapMember(p => p.AggregateId)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            map.MapMember(p => p.TenantId).SetSerializer(new TenantIdSerializer());
            map.MapMember(p => p.Payload).SetSerializer(new JsonElementSerializer());
        });
    }

    private sealed class TenantIdSerializer : SerializerBase<TenantId>
    {
        public override TenantId Deserialize(
            BsonDeserializationContext context,
            BsonDeserializationArgs args) => new(context.Reader.ReadString());

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            TenantId value) => context.Writer.WriteString(value.Value);
    }

    private sealed class JsonElementSerializer : SerializerBase<JsonElement>
    {
        public override JsonElement Deserialize(
            BsonDeserializationContext context,
            BsonDeserializationArgs args)
        {
            using var document = JsonDocument.Parse(context.Reader.ReadString());

            return document.RootElement.Clone();
        }

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            JsonElement value) => context.Writer.WriteString(value.GetRawText());
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Phoenix.Infrastructure.Repository.Mongo.Mapper;
using SportFeedsBridge.Phoenix.Models.Feeds;
using SportFeedsBridge.Phoenix.Domain.Enums;

namespace SportFeedsBridge.Phoenix.Serializers;

public class MessageClassMap : IBsonClassMap
{
    public void Map()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(FeedsMessage)))
            BsonClassMap.RegisterClassMap<FeedsMessage>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIgnoreExtraElementsIsInherited(true);
                cm.MapIdProperty<long>(x => x.MessageId).SetPhoenixIdGenerator<long>();
                cm.GetMemberMap(x => x.Format).SetSerializer(new EnumSerializer<MessageFormat>(BsonType.String));
                cm.GetMemberMap(x => x.Body).SetSerializer(ZipBinaryBsonSerializer.Instance);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(SnapshotMessage)))
            BsonClassMap.RegisterClassMap<SnapshotMessage>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIgnoreExtraElementsIsInherited(true);
                cm.GetMemberMap(x => x.FeedsType).SetSerializer(new EnumSerializer<FeedsType>(BsonType.String));
                cm.MapIdProperty<long>(x => x.SnapshotID).SetPhoenixIdGenerator<long>();
            });
    }
}

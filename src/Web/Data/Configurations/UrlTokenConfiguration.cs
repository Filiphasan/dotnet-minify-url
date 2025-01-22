using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Web.Data.Entities;

namespace Web.Data.Configurations;

public static class UrlTokenConfiguration
{
    public static void Configure()
    {
        BsonClassMap.RegisterClassMap<UrlToken>(u =>
        {
            u.AutoMap();
            u.SetIgnoreExtraElements(true);
            u.MapIdProperty(x => x.Id)
                .SetElementName("_id");
            u.MapProperty(x => x.Token)
                .SetElementName("token")
                .SetIsRequired(true);
            u.MapProperty(x => x.IsUsed)
                .SetElementName("isUsed")
                .SetIsRequired(true)
                .SetDefaultValue(false);
            u.MapProperty(x => x.CreatedAt)
                .SetElementName("createdAt")
                .SetIsRequired(true);
            u.MapProperty(x => x.UsedAt)
                .SetElementName("usedAt")
                .SetIsRequired(false);
        });
    }

    public static List<CreateIndexModel<UrlToken>> CreateIndexes =>
    [
        new(
            Builders<UrlToken>.IndexKeys
                .Ascending(x => x.IsUsed)
                .Ascending(x => x.CreatedAt),
            new CreateIndexOptions { Name = "Ix_Asc_IsUsed_Asc_CreatedAt" }
        ),
        new(
            Builders<UrlToken>.IndexKeys
                .Ascending(x => x.Token),
            new CreateIndexOptions { Name = "Ix_Asc_Token", Unique = true }
        )
    ];
}
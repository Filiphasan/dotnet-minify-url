using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Web.Data.Entities;

namespace Web.Data.Configurations;

public static class UrlShortenConfiguration
{
    public static void Configure()
    {
        BsonClassMap.RegisterClassMap<UrlShorten>(u =>
        {
            u.AutoMap();
            u.SetIgnoreExtraElements(true);
            u.MapIdProperty(x => x.Id)
                .SetElementName("_id");
            u.MapProperty(x => x.Token)
                .SetElementName("token")
                .SetIsRequired(true);
            u.MapProperty(x => x.Url)
                .SetElementName("url")
                .SetIsRequired(true);
            u.MapProperty(x => x.CreatedAt)
                .SetElementName("createdAt")
                .SetIsRequired(true);
            u.MapProperty(x => x.ExpiredAt)
                .SetElementName("expiredAt")
                .SetIsRequired(true);
        });
    }

    public static List<CreateIndexModel<UrlShorten>> CreateIndexes =>
    [
        new(
            Builders<UrlShorten>.IndexKeys
                .Ascending(x => x.Token),
            new CreateIndexOptions { Name = "Ix_Asc_Token" }
        ),
        new(
            Builders<UrlShorten>.IndexKeys
                .Ascending(x => x.Url)
                .Ascending(x => x.ExpiredAt),
            new CreateIndexOptions { Name = "Ix_Asc_Url_Asc_ExpiredAt" }
        )
    ];
}
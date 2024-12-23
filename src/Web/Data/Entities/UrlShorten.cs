using MongoDB.Bson;

namespace Web.Data.Entities;

public class UrlShorten
{
    public ObjectId Id { get; set; }
    public string Token { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
}
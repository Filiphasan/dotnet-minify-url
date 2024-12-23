using MongoDB.Bson;

namespace Web.Data.Entities;

public class UrlToken
{
    public ObjectId Id { get; set; }
    public string Token { get; set; } = null!;
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
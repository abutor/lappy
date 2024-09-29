using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lappy.General.Models;

public class UserSessionModel
{
    [BsonId]
    public string SessionId { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive {  get; set; }
}

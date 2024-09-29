using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Lappy.General.Models;

public class HistoryValue<T> where T : class
{
    [BsonId]
    public ObjectId ObjectId { get; set; }
    public Guid DocumentId { get; set; }
    public required T Current { get; set; }
    public T[] Changes { get; set; } = [];
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Lappy.History;

public class HistoryValue<T> where T : class
{
    [BsonId]
    public ObjectId ObjectId { get; set; }
    public Guid DocumentId { get; set; }
    public T Current { get; set; }
    public T[] Changes { get; set; } = [];
}

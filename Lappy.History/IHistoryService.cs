using MongoDB.Driver;

namespace Lappy.History;

interface IHistoryService
{
    Task<HistoryValue<TEntity>> GetVersionsAsync<TEntity>(Guid id, CancellationToken token) where TEntity : class;
    Task AddVersionAsync<TEntity>(Guid id, TEntity entity, CancellationToken token) where TEntity : class;
}

public class HistoryService(IMongoDatabase _database) : IHistoryService
{
    public async Task AddVersionAsync<TEntity>(Guid id, TEntity entity, CancellationToken token) where TEntity : class
    {
        var collectionName = typeof(TEntity).Name.ToLowerInvariant();
        var collection = _database.GetCollection<HistoryValue<TEntity>>(collectionName);
        var filter = new FilterDefinitionBuilder<HistoryValue<TEntity>>().Eq(x => x.DocumentId, id);
        var update = new UpdateDefinitionBuilder<HistoryValue<TEntity>>()
            .SetOnInsert(x => x.DocumentId, id)
            .SetOnInsert(x => x.Current, entity)
            .Push(x => x.Changes, entity);

        await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, token);
    }

    public async Task<HistoryValue<TEntity>> GetVersionsAsync<TEntity>(Guid id, CancellationToken token) where TEntity : class
    {
        var collectionName = typeof(TEntity).Name.ToLowerInvariant();
        var collection = _database.GetCollection<HistoryValue<TEntity>>(collectionName);
        var filter = new FilterDefinitionBuilder<HistoryValue<TEntity>>().Eq(x => x.DocumentId, id);
        return await collection.Find(filter).FirstOrDefaultAsync(token);
    }
}

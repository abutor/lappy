using Lappy.General.Models;
using MongoDB.Driver;

namespace Lappy.General.Providers;

public interface IHistoryProvider
{
    Task<HistoryValue<TEntity>> GetVersionsAsync<TEntity>(Guid id, CancellationToken token) where TEntity : class;
    Task AddVersionAsync<TEntity>(Guid id, TEntity entity, CancellationToken token) where TEntity : class;
}

public class HistoryProvider(IMongoDatabase _database) : IHistoryProvider
{
    public async Task AddVersionAsync<TEntity>(Guid id, TEntity entity, CancellationToken token) where TEntity : class
    {
        var collection = _database.GetCollection<HistoryValue<TEntity>>(typeof(TEntity).Name.ToLowerInvariant());

        var update = new UpdateDefinitionBuilder<HistoryValue<TEntity>>()
            .SetOnInsert(x => x.DocumentId, id)
            .SetOnInsert(x => x.Current, entity)
            .Push(x => x.Changes, entity);

        await collection.UpdateOneAsync(x => x.DocumentId == id, update, new UpdateOptions { IsUpsert = true }, token);
    }

    public async Task<HistoryValue<TEntity>> GetVersionsAsync<TEntity>(Guid id, CancellationToken token) where TEntity : class
    {
        var collection = _database.GetCollection<HistoryValue<TEntity>>(typeof(TEntity).Name.ToLowerInvariant());
        return await collection.Find(x => x.DocumentId == id).FirstOrDefaultAsync(token);
    }
}

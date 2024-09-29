using Lappy.Core;
using Lappy.Core.Models;
using Lappy.General.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Lappy.General.Providers;

public interface ISessionProvider
{
    Task<Results<UserSessionModel>> GetSessions(string userId, CancellationToken token);
    Task<Result> IsActiveSession(string sessionId, CancellationToken token);
    Task<Result<UserSessionModel>> CreateSession(string userId, CancellationToken token);
    Task<Result> FinishsSession(string sessionId, CancellationToken token);
}

public class SessionProvider(IMongoDatabase _database, UserContext _context) : ISessionProvider
{
    private readonly IMongoCollection<UserSessionModel> _sessions = _database.GetCollection<UserSessionModel>("user_sessions");

    public async Task<Result<UserSessionModel>> CreateSession(string userId, CancellationToken token)
    {
        var session = new UserSessionModel
        {
            UserId = userId,
            SessionId = ObjectId.GenerateNewId().ToString(),
            Origin = _context.Origin,
            IpAddress = _context.IpAddress,
            UserAgent = _context.UserAgent,
            StartedAt = DateTime.UtcNow,
            IsActive = true,
            EndedAt = null
        };

        await _sessions.InsertOneAsync(session, new InsertOneOptions(), token);

        return ResultHelper.Success(session);
    }

    public async Task<Result> FinishsSession(string sessionId, CancellationToken token)
    {
        var update = new UpdateDefinitionBuilder<UserSessionModel>()
            .SetOnInsert(x => x.EndedAt, DateTime.UtcNow)
            .SetOnInsert(x => x.IsActive, true);
        await _sessions.UpdateOneAsync(x => x.SessionId == sessionId, update, new UpdateOptions(), token);
        return ResultHelper.Success();
    }

    public async Task<Results<UserSessionModel>> GetSessions(string userId, CancellationToken token)
    {
        var result = await _sessions.Find(x => x.UserId == userId).ToListAsync(token);
        return ResultHelper.List(result, result.Count, 1, result.Count);
    }

    public async Task<Result> IsActiveSession(string sessionId, CancellationToken token)
    {
        var active = await _sessions.Find(x => x.SessionId == sessionId)
            .Project(x => x.IsActive)
            .FirstOrDefaultAsync(token);
        return active ? ResultHelper.Success() : ResultHelper.Failed(200);
    }
}


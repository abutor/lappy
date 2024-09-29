using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lappy.Core;
using Lappy.Core.Models;
using Lappy.Data;
using Microsoft.EntityFrameworkCore;

namespace Lappy.General.Providers;

    public interface IEntityProvider<TModel, TTableRequest>
        where TModel : class
        where TTableRequest : TableRequest
    {
        Task<Result<TModel>> GetAsync(Guid id, CancellationToken token = default);
        Task<Results<TModel>> GetAllAsync(TTableRequest request, CancellationToken token = default);
        Task<Result<TModel>> CreateAsync(TModel model, CancellationToken token = default);
        Task<Result<TModel>> UpdateAsync(Guid id, TModel model, CancellationToken token = default);
        Task<Result> DeleteAsync(Guid id, CancellationToken token = default);
    }

    public class BaseEntityProvider<TEntity, TModel, TRequest>(DbContext _db, IMapper _mapper, UserContext _userContext, IHistoryProvider _historyProvider) : IEntityProvider<TModel, TRequest>
        where TEntity : BaseEntity
        where TModel : BaseModel
        where TRequest : TableRequest
    {
        public async Task<Results<TModel>> GetAllAsync(TRequest request, CancellationToken token = default)
        {
            var set = _db.Set<TEntity>().Where(x => x.CompanyId == _userContext.CompanyId);
            var count = await set.CountAsync(token);
            var result = await set
                .ProjectTo<TModel>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToArrayAsync(token);

            return ResultHelper.List(result, count, request.Page, request.PageSize);
        }

        public async Task<Result<TModel>> GetAsync(Guid id, CancellationToken token = default)
        {
            var result = await _db.Set<TEntity>()
                .Where(x => x.CompanyId == _userContext.CompanyId && x.Id == id)
                .AsNoTracking()
                .ProjectTo<TModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(token);

            return result == null ? ResultHelper.Failed<TModel>(404) : ResultHelper.Success(result);
        }

        public async Task<Results<TModel>> GetVersionsAsync(Guid id, CancellationToken token = default)
        {
            var set = _db.Set<TEntity>().Where(x => x.CompanyId == _userContext.CompanyId && x.Id == id);

            var result = await set
                .ProjectTo<TModel>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToArrayAsync(token);

            return ResultHelper.List(result, result.Length, 1, 10_000);
        }

        public async Task<Result<TModel>> CreateAsync(TModel model, CancellationToken token = default)
        {
            var entity = _mapper.Map<TEntity>(model);

            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedById = _userContext.UserId;
            entity.CompanyId = _userContext.CompanyId;

            _db.Set<TEntity>().Add(entity);
            await _db.SaveChangesAsync(token);
            await _historyProvider.AddVersionAsync(entity.Id, entity, token);

            return ResultHelper.Success(_mapper.Map<TModel>(entity));
        }

        public async Task<Result<TModel>> UpdateAsync(Guid id, TModel model, CancellationToken token = default)
        {
            var set = _db.Set<TEntity>();
            var recent = await set
                .Where(x => x.CompanyId == _userContext.CompanyId && x.Id == id)
                .AnyAsync(token);

            if (recent is false)
            {
                return ResultHelper.Failed<TModel>(404);
            }

            var entity = _mapper.Map<TEntity>(model);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedById = _userContext.UserId;

            _db.Attach(entity).State = EntityState.Modified;
            _db.Entry(entity).Property(x => x.CreatedAt).IsModified = false;
            _db.Entry(entity).Property(x => x.CreatedById).IsModified = false;
            _db.Entry(entity).Property(x => x.CompanyId).IsModified = false;

            await _db.SaveChangesAsync(token);
            await _historyProvider.AddVersionAsync(entity.Id, entity, token);

            model = _mapper.Map<TModel>(entity);
            return ResultHelper.Success(model);
        }
        public async Task<Result> DeleteAsync(Guid id, CancellationToken token = default)
        {
            var model = await GetAsync(id, token);
            if (model.IsSuccess && model.Value != null)
            {
                model.Value.IsArchived = true;
                return await UpdateAsync(id, model.Value, token);
            }

            return model;
        }
    }

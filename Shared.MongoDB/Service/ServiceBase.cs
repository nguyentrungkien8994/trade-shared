using System.Linq.Expressions;
using MongoDB.Bson;
using Shared.MongoDB.Dto;
using Shared.MongoDB.Entity;
using Shared.MongoDB.Repository;

namespace SIGNAL.STORAGE.SERVICE
{
    public class ServiceBase<T, IRepo> : IServiceBase<T> where T : EntityBase where IRepo : IRepositoryBase<T>
    {
        IRepo _repositoryBase;

        public IRepositoryBase<T> Repository => _repositoryBase;

        public ServiceBase(IRepo repositoryBase)
        {
            _repositoryBase = repositoryBase;
        }

        //public void SetTrackingSystemField(T entity)
        //{
        //    if (entity == null) return;
        //    if (entity.id == Guid.Empty) entity.id = Guid.NewGuid();
        //    entity.created_at = DateTimeHelper.GetUtcTimestampSeconds();
        //    entity.updated_at = DateTimeHelper.GetUtcTimestampSeconds();
        //    entity.created_by = "admin";
        //    entity.updated_by = "admin";
        //}

        public virtual async Task<int> Delete(ObjectId id)
        {
            return await _repositoryBase.Delete(id);
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _repositoryBase.GetAllAsync();
        }

        public virtual async Task<T?> GetAsync(ObjectId id)
        {
            return await _repositoryBase.GetAsync(id);
        }

        public virtual async Task<int> Insert(T entity)
        {
            //SetTrackingSystemField(entity);
            return await _repositoryBase.Insert(entity);
        }

        public virtual async Task<int> InsertRange(T[] entities)
        {
            //foreach (T entity in entities)
                //SetTrackingSystemField(entity);
            return await _repositoryBase.InsertRange(entities);
        }

        public virtual async Task<int> Update(T entity)
        {
            IEntityBase? obj = await _repositoryBase.SearchOne(x=>x.id==entity.id);
            if (obj == null) throw new Exception("Entity not found");
            entity.created_at = entity.created_at;
            entity.created_by = entity.created_by;
            entity.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            entity.updated_by = "admin";
            return await _repositoryBase.Update(entity);
        }

        public virtual async Task<List<T>> Search(Expression<Func<T, bool>> expression)
        {
            return await _repositoryBase.Search(expression);
        }

        public virtual async Task<T?> SearchOne(Expression<Func<T, bool>> expression)
        {
            return await _repositoryBase.SearchOne(expression);
        }

        public async virtual Task<PagingObject<T>> Paging(Expression<Func<T, bool>> expression, int skip, int take)
        {
            return await _repositoryBase.Paging(expression, skip, take);
        }

        public virtual async Task<long> Count(Expression<Func<T, bool>> expression)
        {
            return await _repositoryBase.Count(expression);
        }

        public virtual async Task<int> UpdateRange(T[] entities)
        {
            return await _repositoryBase.UpdateRange(entities);
        }

        public virtual async Task<int> DeleteRange(T[] entities)
        {
            return await _repositoryBase.DeleteRange(entities);
        }

        public Task<PagingObject<T>> Paging<TKey>(Expression<Func<T, bool>> expression, int skip, int take, Expression<Func<T, TKey>>? sort = null)
        {
            throw new NotImplementedException();
        }
    }
    public class ServiceBase<T> : ServiceBase<T, IRepositoryBase<T>> where T : EntityBase
    {
        public ServiceBase(IRepositoryBase<T> repositoryBase) : base(repositoryBase)
        {
        }
    }
}

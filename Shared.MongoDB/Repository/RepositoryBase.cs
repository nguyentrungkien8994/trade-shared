using MongoDB.Bson;
using MongoDB.Driver;
using Shared.MongoDB.Dto;
using Shared.MongoDB.Entity;
using System.Linq.Expressions;

namespace Shared.MongoDB.Repository;

public class RepositoryBase<T> : IRepositoryBase<T> where T : EntityBase, new()
{
    private readonly IMongoCollection<T> _collection;
    MongoDBContext _dbContext;
    public IMongoCollection<T> MongoCollectionDB
    {
        get
        {
            return _collection;
        }
    }
    public RepositoryBase(MongoDBContext dbContext)
    {
        _dbContext = dbContext;
        _collection = dbContext.GetCollection<T>(typeof(T).GetTableName());
    }

    public async Task<int> Delete(ObjectId id)
    {
        T? obj = _collection.FindOneAndDelete(x => x.id == id);
        if (obj == null) return 0;
        return 1;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<T?> GetAsync(ObjectId id)
    {
        //return await _collection.FindAsync(x => x.id == id);
        return await _collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<int> Insert(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return 1;
    }
    public async Task<int> InsertRange(T[] entities)
    {
        await _collection.InsertManyAsync(entities);
        return entities.Length;
    }

    public async Task<int> Update(T entity)
    {
        var replaceResult = await _collection.ReplaceOneAsync<T>(x => x.id == entity.id, entity);
        return 1;
    }

    public async Task<List<T>> Search(Expression<Func<T, bool>> expression)
    {
        return await _collection.Find(expression).ToListAsync();
    }

    public async Task<T?> SearchOne(Expression<Func<T, bool>> expression)
    {
        return await _collection.Find(expression).FirstOrDefaultAsync();
    }

    public async Task<PagingObject<T>> Paging(Expression<Func<T, bool>>? expression, int skip, int take)
    {
        long totals = 0;
        List<T> datas = new List<T>();
        if (expression != null)
        {
            totals = await _collection.CountDocumentsAsync<T>(expression);
            datas = await _collection.Find(expression).SortByDescending(x => x.updated_at).Skip(skip).Limit(take).ToListAsync();
        }
        else
        {
            totals = await _collection.CountDocumentsAsync<T>(_ => true);
            datas = await _collection.Find(_ => true).SortByDescending(x => x.updated_at).Skip(skip).Limit(take).ToListAsync();
        }
        return new PagingObject<T>() { Data = datas, Skip = skip, Take = take, TotalCount = totals };
    }

    public async Task<long> Count(Expression<Func<T, bool>> expression)
    {
        return await _collection.CountDocumentsAsync(_ => true);
    }

    public async Task<int> UpdateRange(T[] entities)
    {
        return 1;
    }

    public Task<TResult?> SearchOnceBySqlRaw<TResult>(string sqlText, params object[] objects)
    {
        throw new NotImplementedException();
    }

    public async Task<PagingObject<T>> Paging<TKey>(Expression<Func<T, bool>> expression, int skip, int take, Expression<Func<T, TKey>>? sort = null)
    {
        long totals = 0;
        List<T> datas = new List<T>();
        if (sort == null)
            return new PagingObject<T>();
        if (expression != null)
        {
            totals = await _collection.CountDocumentsAsync<T>(expression);
            datas = await _collection.Find(expression).SortByDescending(x => x.updated_at).Skip(skip).Limit(take).ToListAsync();
        }
        else
        {
            totals = await _collection.CountDocumentsAsync<T>(_ => true);
            datas = await _collection.Find(_ => true).SortByDescending(x => x.updated_at).Skip(skip).Limit(take).ToListAsync();
        }
        return new PagingObject<T>() { Data = datas, Skip = skip, Take = take, TotalCount = totals };
    }

    public async Task<int> DeleteRange(T[] entities)
    {
        return entities.Length;
    }

    public Task<List<TResult>> SearchBySqlRaw<TResult>(string sqlText, params object[] objects)
    {
        throw new NotImplementedException();
    }

}

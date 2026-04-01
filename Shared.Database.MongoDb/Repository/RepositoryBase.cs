using MongoDB.Driver;
using System.Linq.Expressions;
using Core.Database;
using Core.Database.Entity;

namespace Shared.Database.MongoDb.Repository;

public class RepositoryBase<T, TId> : IRepositoryBase<T, TId> where T : IEntityBase<TId>
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
    public Task<long> CountAsync(Expression<Func<T, bool>> expression)
    {
        throw new NotImplementedException();
    }

    public async Task<int> DeleteAsync(TId id)
    {
        var filter = Builders<T>.Filter.Eq(x => x.id, id);
        var result = _collection.DeleteOneAsync(filter);
        return 1;
    }

    public Task<int> DeleteRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAllAsync()
    {
        return _collection.Find(_ => true).ToListAsync();
    }

    public Task<T> GetAsync(TId id)
    {
        var filter = Builders<T>.Filter.Eq(x => x.id, id);
        return _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<int> InsertAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return 1;
    }

    public Task<int> InsertRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> SearchAsync(Expression<Func<T, bool>> expression)
    {
        return _collection.Find(expression).ToListAsync();
    }

    public Task<List<T>> SearchBySqlRawAsync(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<List<TResult>> SearchBySqlRawAsync<TResult>(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<T> SearchOnceBySqlRawAsync(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> SearchOnceBySqlRawAsync<TResult>(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<T> SearchOneAsync(Expression<Func<T, bool>> expression)
    {
        return _collection.Find(expression).FirstOrDefaultAsync();
    }

    public async Task<int> UpdateAsync(T entity)
    {
        var filter = Builders<T>.Filter.Eq(x => x.id, entity.id);
        var result = await _collection.ReplaceOneAsync(filter, entity);
        return 1;
    }

    public Task<int> UpdateRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }
}

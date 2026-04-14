using MongoDB.Driver;
using System.Linq.Expressions;
using KLib.Core.Database;
using KLib.Core.Database.Entity;
using KLib.Core.Database.Dto;

namespace Shared.Database.MongoDb.Repository;

public class RepositoryBase<T, TId> : IRepositoryBase<T, TId> where T : IEntityKey<TId>
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

    public Task<T?> GetAsync(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<T?> SearchOneAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<T>> SearchAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<PagingObject<T>> PagingAsync(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertBulkAsync(T[] entities)
    {
        throw new NotImplementedException();
    }
}

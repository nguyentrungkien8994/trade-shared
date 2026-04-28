using KLib.Core.Database;
using KLib.Core.Database.Entity;
using Shared.Database.Neo4j.Repository;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Shared.Database.Neo4j.Service
{
    public interface IServiceBaseNeo4j : IServiceBase
    {
        Task<int> UpSertNodeAsync(IEnumerable<IDictionary<string,object>> nodes,string nodeName, string idKey = "id");
        Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id");
        Task<object?> SearchNodeAsync(SearchParam searchParam);
        Task<object?> SearchNodeByCypherRawAsync(string cypher);
    }
    public interface IServiceBaseNeo4j<T, TId, IRepo> where T : IEntityKey<TId> where IRepo : IRepositoryBaseNeo4j<T, TId>
    {
        Task<T> GetAsync(TId id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> InsertAsync(T entity);
        Task<int> DeleteAsync(TId id);
        Task<T> UpdateAsync(T entity);

    }
    public interface IServiceBaseNeo4j<T, TId> : IServiceBaseNeo4j<T, TId, IRepositoryBaseNeo4j<T, TId>> where T : IEntityKey<TId>
    {

    }
    public interface IServiceBaseNeo4j<T> : IServiceBaseNeo4j<T, Guid> where T : IEntityKey<Guid>
    {

    }
}

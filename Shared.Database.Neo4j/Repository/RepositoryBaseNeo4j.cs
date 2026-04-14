using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Shared.Database.Neo4j.Builder;
using Shared.Database.Neo4j.DataAccess;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
namespace Shared.Database.Neo4j.Repository;

public class RepositoryBaseNeo4j : IRepositoryBaseNeo4j
{
    private readonly IDataAccess _dataAccess;
    private readonly ICypherBuilder _cypherBuilder;
    public RepositoryBaseNeo4j(IDataAccess dataAccess, ICypherBuilder cypherBuilder)
    {
        _dataAccess = dataAccess;
        _cypherBuilder = cypherBuilder;
    }
    public Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string nodeName)
    {
        string cypher = _cypherBuilder.BuildGetAll(nodeName);
        return _dataAccess.ReadAsync(cypher);
    }

    public Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string nodeName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string nodeName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<int> UpSertNodeAsync(IEnumerable<IDictionary<string, object>> nodes, string nodeName, string idKey = "id")
    {
        (string cypher, object parameters) = _cypherBuilder.BuildMergeNode(nodes, nodeName, idKey);
        var summary = await _dataAccess.WriteScalarAsync(cypher, parameters);
        return summary.Counters.NodesCreated;
    }

    public virtual async Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        if (rels == null || rels.Count() == 0) return 0;
        (string cypher, object parameters) = _cypherBuilder.BuildMergeRelationship(rels, fromKey, toKey);
        var summary = await _dataAccess.WriteScalarAsync(cypher, parameters);
        return summary.Counters.RelationshipsCreated;
    }
}
public class RepositoryBaseNeo4j<T, TId> : RepositoryBaseNeo4j, IRepositoryBaseNeo4j<T, TId> where T : IEntityKey<TId>
{
    private readonly IDataAccess _dataAccess;
    private readonly ICypherBuilder _cypherBuilder;

    public RepositoryBaseNeo4j(IDataAccess dataAccess, ICypherBuilder cypherBuilder) : base(dataAccess, cypherBuilder)
    {
        _dataAccess = dataAccess;
        _cypherBuilder = cypherBuilder;
    }

    public virtual async Task<int> DeleteAsync(TId id)
    {
        (string cypher, object parameters) = _cypherBuilder.BuildDelete<T>(id);
        var summary = await _dataAccess.WriteScalarAsync(cypher, parameters);
        return summary.Counters.NodesDeleted;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        string cypher = _cypherBuilder.BuildGetAll(typeof(T).GetTableName());
        return await _dataAccess.ReadAsync<T>(cypher);
    }

    public virtual async Task<T?> GetAsync(TId id)
    {
        (string cypher, object parameters) = _cypherBuilder.BuildDelete<T>(id);
        var records = await _dataAccess.ReadAsync<T>(cypher, parameters);
        return records.FirstOrDefault();
    }

    public virtual async Task<T?> InsertAsync(T entity)
    {
        (string cypher, object parameters) = _cypherBuilder.BuildCreate<T>(entity);
        return await _dataAccess.WriteAsync<T>(cypher, parameters);
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        (string cypher, object parameters) = _cypherBuilder.BuildUpdate<T>(entity);
        return await _dataAccess.WriteAsync<T>(cypher, parameters);
    }

    public virtual async Task<object?> SearchNode(CypherQuery cypher)
    {
        if (cypher == null) return null;
        var records = await _dataAccess.WriteAsync(cypher.Query, cypher.Params);
        Utils utils = new();
        var results = utils.ParserRecords(records);
        return results;
    }

    public virtual async Task<int> UpSertNodeAsync(IEnumerable<object> upserts, string idKey = "id")
    {
        if (upserts == null || upserts.Count() == 0) return 0;
        (string cypher, object parameters) = _cypherBuilder.BuildMergeNode<T>(upserts, idKey);
        var summary = await _dataAccess.WriteScalarAsync(cypher, parameters);
        return summary.Counters.NodesCreated;
    }
}

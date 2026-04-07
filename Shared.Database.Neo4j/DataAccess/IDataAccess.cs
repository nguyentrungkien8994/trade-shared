using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Database.Neo4j.DataAccess
{
    public interface IDataAccess
    {
        // READ
        Task<IEnumerable<T>> ReadAsync<T>(string cypher,object? parameters = null);
        Task<IEnumerable<IRecord>> ReadMultipleNodeAsync(string cypher, object? parameters = null);

        // WRITE
        Task<T?> WriteAsync<T>(string cypher, object? parameters = null);
        Task<IEnumerable<IRecord>> WriteAsync(string cypher, object? parameters);
        Task<IResultSummary> WriteScalarAsync(string cypher, object? parameters);
    }
}

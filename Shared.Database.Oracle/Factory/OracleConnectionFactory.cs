using System.Data;
using Oracle.ManagedDataAccess.Client;
namespace Shared.Database.Oracle.Factory;

public class OracleConnectionFactory : IOracleConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateAsync()
    {
        var conn = new  OracleConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
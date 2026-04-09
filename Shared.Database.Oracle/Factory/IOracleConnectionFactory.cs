using System.Data;

namespace Shared.Database.Oracle.Factory;

public interface IOracleConnectionFactory
{
    Task<IDbConnection> CreateAsync();
}

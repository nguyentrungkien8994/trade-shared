using System.Reflection;

namespace Shared.Database.Oracle.Builder;

public interface IOracleSqlBuilder
{
    (string sql, object param) BuildInsert<T>(T entity);
    (string sql, object param) BuildUpdate<T>(T entity, string idField = "id");
    (string sql, object param) BuildDelete<T, TId>(TId id, string idField = "id");
    (string sql, object param) BuildGetById<T, TId>(TId id, string idField = "id");
    string BuildGetAll(string tableName);
    string BuildWhere(object filter,
    Dictionary<string, object> parameters,
    SqlParamContext ctx);
    string BuildWhere<T>(object filter,
    List<PropertyInfo> props,
    Dictionary<string, object> parameters,
    SqlParamContext ctx);

    (string sql, object param) BuildPaging<T>(int skip, int take);
    (string sql, object param) BuildPaging<T>(
        int skip,
        int take,
        IDictionary<string, object>? filters = null,
        IEnumerable<(string field, bool desc)>? sort = null
    );
    (string sql, object param) BuildPaging(
        string tableName,
        int skip,
        int take,
        IDictionary<string, object>? filters = null,
        IEnumerable<(string field, bool desc)>? sort = null
    );

    (string sql, object param) BuildMerge<T>(T entity, string idField = "id");

    (string sql, object param) BuildBulkInsert<T>(IEnumerable<T> entities);
}

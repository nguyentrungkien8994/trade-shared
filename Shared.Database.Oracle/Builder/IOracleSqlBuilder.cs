namespace Shared.Database.Oracle.Builder;

public interface IOracleSqlBuilder
{
    (string sql, object param) BuildInsert<T>(T entity);
    (string sql, object param) BuildUpdate<T>(T entity, string idField = "id");
    (string sql, object param) BuildDelete<T, TId>(TId id, string idField = "id");

    (string sql, object param) BuildGetById<T, TId>(TId id, string idField = "id");
    string BuildGetAll<T>();

    (string sql, object param) BuildWhere<T>(IDictionary<string, object> filters);

    (string sql, object param) BuildPaging<T>(int skip, int take);

    (string sql, object param) BuildMerge<T>(T entity, string idField = "id");

    (string sql, object param) BuildBulkInsert<T>(IEnumerable<T> entities);
}

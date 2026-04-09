using Core.Database;
using Dapper;
using System.Text;
namespace Shared.Database.Oracle.Builder;

public class OracleSqlBuilder : IOracleSqlBuilder
{
    // ================= INSERT =================
    public (string sql, object param) BuildInsert<T>(T entity)
    {
        var type = typeof(T);
        var table = type.GetTableName;
        var props = EntityMetadata.GetProperties(type);

        var columns = props.Select(EntityMetadata.GetColumnName);
        var values = props.Select(p => $":{p.Name}");

        var sql = $@"
INSERT INTO {table}
({string.Join(",", columns)})
VALUES ({string.Join(",", values)})";

        return (sql, entity!);
    }

    // ================= UPDATE =================
    public (string sql, object param) BuildUpdate<T>(T entity, string idField = "id")
    {
        var type = typeof(T);
        var table = EntityMetadata.GetTableName(type);
        var props = EntityMetadata.GetProperties(type);

        var set = props
            .Where(p => !p.Name.Equals(idField, StringComparison.OrdinalIgnoreCase))
            .Select(p => $"{EntityMetadata.GetColumnName(p)} = :{p.Name}");

        var sql = $@"
UPDATE {table}
SET {string.Join(",", set)}
WHERE {idField.ToUpper()} = :{idField}";

        return (sql, entity!);
    }

    // ================= DELETE =================
    public (string sql, object param) BuildDelete<T, TId>(TId id, string idField = "id")
    {
        var table = EntityMetadata.GetTableName(typeof(T));

        var sql = $"DELETE FROM {table} WHERE {idField.ToUpper()} = :id";

        return (sql, new { id });
    }

    // ================= GET BY ID =================
    public (string sql, object param) BuildGetById<T, TId>(TId id, string idField = "id")
    {
        var table = EntityMetadata.GetTableName(typeof(T));

        var sql = $"SELECT * FROM {table} WHERE {idField.ToUpper()} = :id";

        return (sql, new { id });
    }

    // ================= GET ALL =================
    public string BuildGetAll<T>()
    {
        var table = EntityMetadata.GetTableName(typeof(T));
        return $"SELECT * FROM {table}";
    }

    // ================= WHERE =================
    public (string sql, object param) BuildWhere<T>(IDictionary<string, object> filters)
    {
        var table = EntityMetadata.GetTableName(typeof(T));

        var conditions = filters.Keys
            .Select(k => $"{k.ToUpper()} = :{k}");

        var sql = $@"
SELECT * FROM {table}
WHERE {string.Join(" AND ", conditions)}";

        return (sql, filters);
    }

    // ================= PAGING =================
    public (string sql, object param) BuildPaging<T>(int skip, int take)
    {
        var table = EntityMetadata.GetTableName(typeof(T));

        var sql = $@"
SELECT * FROM {table}
OFFSET :skip ROWS FETCH NEXT :take ROWS ONLY";

        return (sql, new { skip, take });
    }

    // ================= MERGE =================
    public (string sql, object param) BuildMerge<T>(T entity, string idField = "id")
    {
        var type = typeof(T);
        var table = EntityMetadata.GetTableName(type);
        var props = EntityMetadata.GetProperties(type);

        var columns = props.Select(EntityMetadata.GetColumnName).ToList();

        var updateSet = props
            .Where(p => !p.Name.Equals(idField, StringComparison.OrdinalIgnoreCase))
            .Select(p => $"target.{EntityMetadata.GetColumnName(p)} = source.{EntityMetadata.GetColumnName(p)}");

        var insertCols = string.Join(",", columns);
        var insertVals = string.Join(",", columns.Select(c => $"source.{c}"));

        var selectSource = string.Join(",",
            props.Select(p => $":{p.Name} AS {EntityMetadata.GetColumnName(p)}"));

        var sql = $@"
MERGE INTO {table} target
USING (SELECT {selectSource} FROM DUAL) source
ON (target.{idField.ToUpper()} = source.{idField.ToUpper()})
WHEN MATCHED THEN
    UPDATE SET {string.Join(",", updateSet)}
WHEN NOT MATCHED THEN
    INSERT ({insertCols})
    VALUES ({insertVals})";

        return (sql, entity!);
    }

    // ================= BULK INSERT =================
    public (string sql, object param) BuildBulkInsert<T>(IEnumerable<T> entities)
    {
        var list = entities.ToList();
        var type = typeof(T);

        var table = EntityMetadata.GetTableName(type);
        var props = EntityMetadata.GetProperties(type);

        var columns = string.Join(",", props.Select(EntityMetadata.GetColumnName));

        var sb = new StringBuilder();
        var param = new DynamicParameters();

        sb.Append($"INSERT INTO {table} ({columns}) ");

        for (int i = 0; i < list.Count; i++)
        {
            var prefix = $"p{i}_";

            var values = new List<string>();

            foreach (var prop in props)
            {
                var name = prefix + prop.Name;
                values.Add($":{name}");
                param.Add(name, prop.GetValue(list[i]));
            }

            sb.Append($"SELECT {string.Join(",", values)} FROM DUAL");

            if (i < list.Count - 1)
                sb.Append(" UNION ALL ");
        }

        return (sb.ToString(), param);
    }
}
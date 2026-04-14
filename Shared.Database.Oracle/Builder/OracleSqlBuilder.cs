using KLib.Core.Database;
using Dapper;
using System.Reflection;
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
    public string BuildGetAll(string tableName)
    {
        return $"SELECT * FROM {tableName}";
    }

    // ================= WHERE =================
    private string BuildFieldCondition(
    string field,
    object value,
    Dictionary<string, object> parameters,
    SqlParamContext ctx)
    {


        if (value is IDictionary<string, object> ops)
        {
            var conditions = new List<string>();

            foreach (var (op, val) in ops)
            {
                var paramName = ctx.Next($"p_{field}");

                var condition = op switch
                {
                    "$eq" => $"{field} = :{paramName}",
                    "$neq" => $"{field} <> :{paramName}",
                    "$gt" => $"{field} > :{paramName}",
                    "$gte" => $"{field} >= :{paramName}",
                    "$lt" => $"{field} < :{paramName}",
                    "$lte" => $"{field} <= :{paramName}",
                    "$like" => $"{field} LIKE :{paramName}",
                    "$in" => $"{field} IN :{paramName}",
                    "$null" => $"{field} IS NULL",
                    "$notnull" => $"{field} IS NOT NULL",
                    "$between" => BuildBetween(field, val, parameters, ctx),
                    _ => throw new NotSupportedException(op)
                };

                if (op != "$null" && op != "$notnull")
                    parameters[paramName] = val;

                conditions.Add(condition);
            }

            return string.Join(" AND ", conditions);
        }

        // shorthand
        var name = ctx.Next($"p_{field}");
        parameters[name] = value;

        return $"{field} = :{name}";
    }
    private string BuildFieldCondition(
    string field,
    object value,
    List<PropertyInfo> props,
    Dictionary<string, object> parameters,
    SqlParamContext ctx)
    {
        var prop = props.FirstOrDefault(p =>
            p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));

        if (prop == null)
            throw new Exception($"Invalid field: {field}");

        var column = EntityMetadata.GetColumnName(prop);

        if (value is IDictionary<string, object> ops)
        {
            var conditions = new List<string>();

            foreach (var (op, val) in ops)
            {
                var paramName = ctx.Next($"p_{field}");

                var condition = op switch
                {
                    "$eq" => $"{column} = :{paramName}",
                    "$neq" => $"{column} <> :{paramName}",
                    "$gt" => $"{column} > :{paramName}",
                    "$gte" => $"{column} >= :{paramName}",
                    "$lt" => $"{column} < :{paramName}",
                    "$lte" => $"{column} <= :{paramName}",
                    "$like" => $"{column} LIKE :{paramName}",
                    "$in" => $"{column} IN :{paramName}",
                    "$null" => $"{column} IS NULL",
                    "$notnull" => $"{column} IS NOT NULL",
                    "$between" => BuildBetween(column, val, parameters, ctx),
                    _ => throw new NotSupportedException(op)
                };

                if (op != "$null" && op != "$notnull")
                    parameters[paramName] = val;

                conditions.Add(condition);
            }

            return string.Join(" AND ", conditions);
        }

        // shorthand
        var name = ctx.Next($"p_{field}");
        parameters[name] = value;

        return $"{column} = :{name}";
    }

    private string BuildBetween(
    string column,
    object value,
    Dictionary<string, object> parameters,
    SqlParamContext ctx)
    {
        var arr = (object[])value;

        var p1 = ctx.Next("bt");
        var p2 = ctx.Next("bt");

        parameters[p1] = arr[0];
        parameters[p2] = arr[1];

        return $"{column} BETWEEN :{p1} AND :{p2}";
    }
    public string BuildWhere(object filter,
    Dictionary<string, object> parameters,
    SqlParamContext ctx)
    {
        if (filter is IDictionary<string, object> dict)
        {
            var conditions = new List<string>();

            foreach (var (key, value) in dict)
            {
                if (key == "$and" || key == "$or")
                {
                    var items = (object[])value;

                    var sub = items.Select(item =>
                        "(" + BuildWhere(item, parameters, ctx) + ")"
                    );

                    var join = key == "$or" ? " OR " : " AND ";

                    conditions.Add(string.Join(join, sub));
                }
                else
                {
                    conditions.Add(
                        BuildFieldCondition(key, value, parameters, ctx)
                    );
                }
            }

            return string.Join(" AND ", conditions);
        }

        throw new Exception("Invalid filter");
    }
    public string BuildWhere<T>(object filter,
    List<PropertyInfo> props,
    Dictionary<string, object> parameters,
    SqlParamContext ctx)
    {
        if (filter is IDictionary<string, object> dict)
        {
            var conditions = new List<string>();

            foreach (var (key, value) in dict)
            {
                if (key == "$and" || key == "$or")
                {
                    var items = (object[])value;

                    var sub = items.Select(item =>
                        "(" + BuildWhere<T>(item, props, parameters, ctx) + ")"
                    );

                    var join = key == "$or" ? " OR " : " AND ";

                    conditions.Add(string.Join(join, sub));
                }
                else
                {
                    conditions.Add(
                        BuildFieldCondition(key, value, props, parameters, ctx)
                    );
                }
            }

            return string.Join(" AND ", conditions);
        }

        throw new Exception("Invalid filter");
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
    public (string sql, object param) BuildPaging(string tableName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        var table = tableName;
        var parameters = new Dictionary<string, object>
        {
            ["skip"] = skip,
            ["take"] = take
        };
        var ctx = new SqlParamContext();
        var sql = new StringBuilder();
        sql.Append($"SELECT * FROM {table}");

        // ================= WHERE =================
        if (filters != null && filters.Count > 0)
        {
            var where = BuildWhere(filters, parameters, ctx);
            sql.Append(" WHERE " + where);
        }

        // ================= ORDER =================
        if (sort != null && sort.Any())
        {
            var order = sort.Select(s =>
            {
                return $"{s.field} {(s.desc ? "DESC" : "ASC")}";
            });

            sql.Append(" ORDER BY " + string.Join(",", order));
        }
        else
        {
            sql.Append(" ORDER BY 1");
        }

        // ================= PAGING =================
        sql.Append(" OFFSET :skip ROWS FETCH NEXT :take ROWS ONLY");

        return (sql.ToString(), parameters);
    }
    public (string sql, object param) BuildPaging<T>(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        var type = typeof(T);
        var table = EntityMetadata.GetTableName(type);
        var props = EntityMetadata.GetProperties(type);

        var parameters = new Dictionary<string, object>
        {
            ["skip"] = skip,
            ["take"] = take
        };
        var ctx = new SqlParamContext();
        var sql = new StringBuilder();
        sql.Append($"SELECT * FROM {table}");

        // ================= WHERE =================
        if (filters != null && filters.Count > 0)
        {
            var where = BuildWhere<T>(filters, props, parameters, ctx);
            sql.Append(" WHERE " + where);
        }

        // ================= ORDER =================
        if (sort != null && sort.Any())
        {
            var order = sort.Select(s =>
            {
                var prop = props.First(p => p.Name.Equals(s.field, StringComparison.OrdinalIgnoreCase));
                var col = EntityMetadata.GetColumnName(prop);
                return $"{col} {(s.desc ? "DESC" : "ASC")}";
            });

            sql.Append(" ORDER BY " + string.Join(",", order));
        }
        else
        {
            sql.Append(" ORDER BY 1");
        }

        // ================= PAGING =================
        sql.Append(" OFFSET :skip ROWS FETCH NEXT :take ROWS ONLY");

        return (sql.ToString(), parameters);
    }


}
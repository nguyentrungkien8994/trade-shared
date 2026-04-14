using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KLib.Core.Database;

public static class EntityExtension
{
    public static string GetTableName(this Type type)
    {
        var attr = type.GetCustomAttribute<TableAttribute>();
        return attr?.Name ?? type.Name;
    }
    public static string GetIdName(this Type type)
    {
        var keyProp = type
                    .GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any());
        if (keyProp == null)
            throw new Exception("Not found any key!");

        var columnAttr = keyProp.GetCustomAttribute<ColumnAttribute>();
        var columnName = columnAttr?.Name ?? keyProp.Name;
        return columnName;
    }
}

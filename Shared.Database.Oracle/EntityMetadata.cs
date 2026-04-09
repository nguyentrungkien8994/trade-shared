using Shared.Database.Oracle.Attributes;
using System.Reflection;
namespace Shared.Database.Oracle;
public static class EntityMetadata
{
    public static string GetTableName(Type type)
    {
        var attr = type.GetCustomAttribute<TableAttribute>();
        return attr?.Name ?? type.Name.ToUpper();
    }

    public static List<PropertyInfo> GetProperties(Type type)
    {
        return type.GetProperties()
            .Where(p => p.CanRead &&
                        p.CanWrite &&
                        !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
            .ToList();
    }

    public static string GetColumnName(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<ColumnAttribute>();
        return attr?.Name ?? prop.Name.ToUpper();
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Shared.MongoDB;

public static class EntityExtension
{
    public static string GetTableName(this Type type)
    {
        var attr = type.GetCustomAttribute<TableAttribute>();
        return attr?.Name ?? type.Name;
    }
}

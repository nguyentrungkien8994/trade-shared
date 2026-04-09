
namespace Shared.Database.Oracle.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : System.Attribute
{
    public string Name { get; }
    public ColumnAttribute(string name) => Name = name;
}

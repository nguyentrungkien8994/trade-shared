namespace Shared.Database.Oracle.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : System.Attribute
{

    public string Name { get; }
    public TableAttribute(string name) => Name = name;
}

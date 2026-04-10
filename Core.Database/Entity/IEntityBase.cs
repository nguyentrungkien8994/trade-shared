namespace KLib.Core.Database.Entity;
public interface IEntityBase<TId>
{
    TId id { get; }
    string created_by { get; }
    string updated_by { get; }
    long created_at { get; }
    long updated_at { get; }
}
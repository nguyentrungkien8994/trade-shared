namespace KLib.Core.Database.Entity;

public interface IEntityTracking
{
    string created_by { get; set; }
    string updated_by { get; set; }
    long created_at { get; set; }
    long updated_at { get; set; }
}

namespace KLib.Core.Database.Entity;
public interface IEntityKey<TId>
{
    TId entityId { get; set; }
}
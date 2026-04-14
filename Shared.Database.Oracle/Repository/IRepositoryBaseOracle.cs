using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;

namespace Shared.Database.Oracle.Repository;

public interface IRepositoryBaseOracle<T, TId> : IRepositoryBase<T, TId> where T : IEntityKey<TId>
{
}
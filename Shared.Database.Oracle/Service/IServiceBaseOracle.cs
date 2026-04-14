using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Shared.Database.Oracle.Repository;
namespace Shared.Database.Oracle.Service;

public interface IServiceBaseOracle : IServiceBase
{

}
public interface IServiceBaseOracle<T, TId, IRepo> : IServiceBase<T, TId, IRepo> where T : IEntityKey<TId> where IRepo : IRepositoryBaseOracle<T, TId>
{
   
}
public interface IServiceBaseOracle<T, TId> : IServiceBaseOracle<T, TId, IRepositoryBaseOracle<T, TId>> where T : IEntityKey<TId>
{

}
public interface IServiceBaseOracle<T> : IServiceBaseOracle<T, Guid> where T : IEntityKey<Guid>
{

}
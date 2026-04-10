using Core.Database;
using Core.Database.Dto;
using Core.Database.Entity;
using Shared.Database.Oracle.Repository;
namespace Shared.Database.Oracle.Service;

public interface IServiceBaseOracle<T, TId, IRepo> : IServiceBase<T, TId, IRepo> where T : IEntityBase<TId> where IRepo : IRepositoryBaseOracle<T, TId>
{
    Task<PagingObject<T>> GetPaging(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<IEnumerable<T>> Search(IDictionary<string, object>? filters = null);
}
public interface IServiceBaseOracle<T, TId> : IServiceBaseOracle<T, TId, IRepositoryBaseOracle<T, TId>> where T : IEntityBase<TId>
{

}
public interface IServiceBaseOracle<T> : IServiceBaseOracle<T, Guid> where T : IEntityBase<Guid>
{

}
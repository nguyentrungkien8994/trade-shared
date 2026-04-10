using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;

namespace Shared.Database.Oracle.Repository;

public interface IRepositoryBaseOracle<T, TId> : IRepositoryBase<T, TId> where T : IEntityBase<TId>
{
    Task<PagingObject<T>> GetPaging(int skip, int take, IDictionary<string, object>? filters=null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<IEnumerable<T>> Search(IDictionary<string, object>? filters=null);
}

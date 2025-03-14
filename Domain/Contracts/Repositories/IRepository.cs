using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Domain.Models.Queries;
using VirtualQueueApi.Domain.Models.Results;

namespace VirtualQueueApi.Domain.Contracts.Repositories;

public interface IRepository<T, TType> where T : EntityBase<TType>
{
    Task<T?> GetById(TType id, CancellationToken ct = default);
    Task<bool> HasAny(Expression<Func<T, bool>> expression, CancellationToken ct = default);
    Task<bool> HasAny(Search<T, TType> search, CancellationToken ct = default);
    Task<T?> Get(Expression<Func<T, bool>>? expression = null, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default);
    Task<T?> Get(Search<T, TType> search, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default);
    Task<IList<T>> GetList(Expression<Func<T, bool>>? expression = null, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default);
    Task<IList<T>> GetList(Search<T, TType> search, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default);
    Task<Dto?> Get<Dto>(Search<T, TType> search, CancellationToken ct = default);
    Task<IList<Dto>> GetList<Dto>(Search<T, TType> search, CancellationToken ct = default);
    Task<PagedResult<Dto>> GetPaged<Dto>(Search<T, TType> search, CancellationToken ct = default);        
    Task Add(T entity, CancellationToken ct = default);
    Task SaveChanges(CancellationToken ct = default);
}

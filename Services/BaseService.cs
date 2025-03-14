using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Contracts.Transactions;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Domain.Models.Queries;
using VirtualQueueApi.Domain.Models.Results;

namespace VirtualQueueApi.Services;

public class BaseService<T, TType> : IBaseService<T, TType> where T : EntityBase<TType>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<T, TType> _repository;
    public BaseService(IRepository<T, TType> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }        
    public async Task<T?> GetById(TType id, CancellationToken ct = default)
    {
        return await _repository.GetById(id, ct);
    }
    public async Task<bool> HasAny(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        var result = await _repository.HasAny(expression, ct);
        return result;
    }
    public async Task<bool> HasAny(Search<T, TType> search, CancellationToken ct = default)
    {
        var result = await _repository.HasAny(search.CreateExpression(), ct);
        return result;
    }        
    public async Task<T?> Get(Expression<Func<T, bool>>? expression = null, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {            
        var result = await _repository.Get(expression, track, include, ct);
        return result;
    }
    public async Task<T?> Get(Search<T, TType> search, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var result = await _repository.Get(search.CreateExpression(), track, include, ct);            
        return result;
    }
    public async Task<IList<T>> GetList(Expression<Func<T, bool>>? expression = null, bool track = true,
       Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var result = await _repository.GetList(expression, track, include, ct);
        return result;
    }
    public async Task<IList<T>> GetList(Search<T, TType> search, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var result = await _repository.GetList(search.CreateExpression(), track, include, ct);
        return result;
    }
    public async Task<Dto?> Get<Dto>(Search<T, TType> search, CancellationToken ct = default)
    {
        var result = await _repository.Get<Dto>(search, ct);
        return result;
    }
    public async Task<IList<Dto>> GetList<Dto>(Search<T, TType> search, CancellationToken ct = default)
    {
        var result = await _repository.GetList<Dto>(search, ct);
        return result;
    }
    public async Task<PagedResult<Dto>> GetPaged<Dto>(Search<T, TType> search, CancellationToken ct = default)
    {
        var result = await _repository.GetPaged<Dto>(search, ct);
        return result;
    }
    public async Task Add(T entity, CancellationToken ct = default)
    {
        await _repository.Add(entity, ct);
    }
    public async Task<int> Commit(CancellationToken ct = default)
    {
        return await _unitOfWork.Commit(ct);
    }
}

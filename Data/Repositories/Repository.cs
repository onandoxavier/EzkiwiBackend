using AutoMapper;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Domain.Models.Queries;
using VirtualQueueApi.Domain.Models.Results;

namespace VirtualQueueApi.Data.Repositories;

public class Repository<T, TType> : IRepository<T, TType> where T : EntityBase<TType>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    private readonly IMapper _mapper;

    public Repository(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;

        _dbSet = _dbContext.Set<T>();
    }
    public async Task<T?> GetById(TType id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(id, ct);
    }
    public async Task<bool> HasAny(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        var result = await _dbSet.AnyAsync(expression, ct);
        return result;
    }
    public async Task<bool> HasAny(Search<T, TType> search, CancellationToken ct = default)
    {
        var result = await _dbSet.AnyAsync(search.CreateExpression(), ct);
        return result;
    }        
    private IQueryable<T> MountQuery(Expression<Func<T, bool>>? expression = null, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet;

        if (include != null) query = include(query);
        if (expression != null) query = query.Where(expression);

        query = track ? query.AsTracking() : query.AsNoTrackingWithIdentityResolution();

        return query;
    }
    public async Task<T?> Get(Expression<Func<T, bool>>? expression = null, bool track = true, 
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var query = MountQuery(expression, track, include, ct);
        var result = await query.FirstOrDefaultAsync(ct);

        return result;
    }
    public async Task<T?> Get(Search<T, TType> search, bool track = true, 
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var query = MountQuery(search.CreateExpression(), track, include, ct);
        if (search.OrderBy != null) query = search.OrderBy(query);

        var result = await query.FirstOrDefaultAsync(ct);

        return result;
    }
    public async Task<IList<T>> GetList(Expression<Func<T, bool>>? expression = null, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var query = MountQuery(expression, track, include, ct);
        var result = await query.ToListAsync(ct);

        return result;
    }
    public async Task<IList<T>> GetList(Search<T, TType> search, bool track = true,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken ct = default)
    {
        var query = MountQuery(search.CreateExpression(), track, include, ct);
        if (search.OrderBy != null) query = search.OrderBy(query);
        var result = await query.ToListAsync(ct);

        return result;
    }
    public async Task<Dto?> Get<Dto>(Search<T, TType> search, CancellationToken ct = default)
    {
        var entity = _dbSet.Where(search.CreateExpression()).AsNoTrackingWithIdentityResolution();
        
        if (search.OrderBy != null) entity = search.OrderBy(entity);
        
        var projected = _mapper.ProjectTo<Dto>(entity);
        var result = await projected.FirstOrDefaultAsync(ct);

        return result;
    }
    public async Task<IList<Dto>> GetList<Dto>(Search<T, TType> search, CancellationToken ct = default)
    {
        var entity = _dbSet.Where(search.CreateExpression()).AsNoTrackingWithIdentityResolution();

        if (search.OrderBy != null) entity = search.OrderBy(entity);

        var projected = _mapper.ProjectTo<Dto>(entity);
        var result = await projected.ToListAsync(ct);

        return result;
    }
    public async Task<PagedResult<Dto>> GetPaged<Dto>(Search<T, TType> search, CancellationToken ct = default)
    {
        var entity = _dbSet.Where(search.CreateExpression()).AsNoTrackingWithIdentityResolution();

        var total = await entity.CountAsync(ct);

        var ordered = search.OrderBy != null ? search.OrderBy(entity) : entity;
        var paged = ordered.Skip(search.SkipPages()).Take(search.Rows);

        var projected = _mapper.ProjectTo<Dto>(paged);
        var items = await projected.ToListAsync(ct);

        var result = new PagedResult<Dto>(items: items, total: total, page: search.Page, search.Rows);

        return result;
    }        
    public async Task Add(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }
    public async Task SaveChanges(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}

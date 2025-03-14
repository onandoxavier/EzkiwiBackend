using AutoMapper;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Data.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken, int>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
}

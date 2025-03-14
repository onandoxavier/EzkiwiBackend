using AutoMapper;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Repositories;

public class UserRepository : Repository<User, int>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
    public async Task<User?> GetByEmail(string email, CancellationToken ct = default)
    {
        return await Get(expression: u => u.Email == email, ct: ct);
    }
}

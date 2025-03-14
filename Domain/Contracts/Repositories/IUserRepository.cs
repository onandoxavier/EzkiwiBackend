using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Contracts.Repositories;
public interface IUserRepository : IRepository<User, int> 
{
    Task<User?> GetByEmail(string email, CancellationToken ct = default);
}




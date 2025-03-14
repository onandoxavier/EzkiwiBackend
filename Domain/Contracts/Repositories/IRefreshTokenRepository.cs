using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Domain.Contracts.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken, int> { }
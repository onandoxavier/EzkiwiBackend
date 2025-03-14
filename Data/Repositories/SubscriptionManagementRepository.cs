using AutoMapper;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Data.Repositories;

public class SubscriptionManagementRepository : Repository<SubscriptionManagement, Guid>, ISubscriptionManagementRepository
{
    public SubscriptionManagementRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
}

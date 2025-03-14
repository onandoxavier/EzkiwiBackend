using AutoMapper;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Repositories;

public class QueueRepository : Repository<Queue, int>, IQueueRepository
{
    public QueueRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
}
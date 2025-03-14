using AutoMapper;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Repositories;

public class PasswordResetFlowRepository : Repository<PasswordResetFlow, Guid>, IPasswordResetFlowRepository
{
    public PasswordResetFlowRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }  
}
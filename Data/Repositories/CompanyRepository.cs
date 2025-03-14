using AutoMapper;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Repositories;

public class CompanyRepository : Repository<Company, int>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
}
using AutoMapper;
using VirtualQueueApi.Domain.Models.Results.CompanyResults;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Profiles;

public class CompanyProfile : Profile
{
    public CompanyProfile()
    {
        CreateMap<Company, CompanyResult>();       
    }
}

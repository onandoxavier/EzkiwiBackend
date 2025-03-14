using AutoMapper;
using VirtualQueueApi.Domain.Models.Results.UserResults;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResult>()
            .ForMember(dto => dto.CompanyName, map => map.MapFrom(u => u.Company.Name));
    }
}

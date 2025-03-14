using VirtualQueueApi.Domain.Models.Commands.UserCommands;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Contracts.Services;

public interface IUserService : IBaseService<User, int> 
{
    Task<User> UpdateUserProfile(UpdateUserProfileCommand command, int userId, CancellationToken ct);
}

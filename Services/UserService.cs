using Microsoft.EntityFrameworkCore;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Contracts.Transactions;
using VirtualQueueApi.Domain.Models.Commands.UserCommands;
using VirtualQueueApi.Models.Entities;
using VirtualQueueApi.Utils.Exceptions;

namespace VirtualQueueApi.Services;

public class UserService : BaseService<User, int>, IUserService
{
    public UserService(IUserRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork) { }

    public async Task<User> UpdateUserProfile(UpdateUserProfileCommand command, int userId, CancellationToken ct)
    {
        var user = await Get(ct: ct,
            expression: u => u.Id == userId,
            include: source => source.Include(x => x.Company));

        if (user == null) throw new EntityNotFoundException("User not found");

        user.Name = command.Name;
        user.Email = command.Email;
        user.Company.Name = command.CompanyName;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await Commit();

        return user;
    }
}

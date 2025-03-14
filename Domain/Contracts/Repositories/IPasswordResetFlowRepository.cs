using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Contracts.Repositories;

public interface IPasswordResetFlowRepository : IRepository<PasswordResetFlow, Guid> { }
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Domain.Contracts.Transactions
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> Commit(CancellationToken ct = default);
        IRepository<T, TType> Repository<T, TType>() where T : EntityBase<TType>;
    }
}

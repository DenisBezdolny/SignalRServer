using GMB.Domain.Entities;

namespace GMB.Domain.Repositories.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByConnectionIdAsync(string connectionId);
        Task<IEnumerable<Client>> GetActiveClientsAsync();
    }
}
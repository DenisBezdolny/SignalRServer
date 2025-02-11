using GMB.Domain.Entities;

namespace GMB.Domain.Repositories.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByConnectionIdAsNoTrackingAsync(string connectionId);
        Task<Client?> GetByConnectionIdAsync(string connectionId);
        Task<IEnumerable<Client>> GetActiveClientsAsync();
        Task<IEnumerable<Client>> GetClientsInRoom(string roomCode);
        Task<Client?> GetLastJoinedClientAsync(string roomCode);
    }
}
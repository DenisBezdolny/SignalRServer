using GMB.Domain.Entities;

namespace GMB.Domain.Repositories.Interfaces
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<Room?> GetRoomByCodeAsync(string code);
        Task<IEnumerable<Room>> GetActiveRoomsAsync(int maxPlayers);
        Task<Room?> GetRoomByClientAsync(Client client);
    }
}

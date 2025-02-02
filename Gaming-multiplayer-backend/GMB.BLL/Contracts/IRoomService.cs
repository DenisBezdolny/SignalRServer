using GMB.Domain.Entities;

namespace GMB.BLL.Contracts
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(Guid id);
        Task<Room?> GetRoomByCodeAsync(string code);
        Task AddRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(Guid id);
        Task<IEnumerable<Room>> GetActiveRoomsAsync();
        Task<Room?> GetRoomByClientAsync(Client client);
        Task<Room?> AssignPlayerToRoomAsync(Client client, int maxRoomSize);
        Task<Room?> JoinRoomByCodeAsync(string code, Client client, int maxRoomSize);
    }
}

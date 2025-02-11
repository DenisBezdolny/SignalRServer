using GMB.Domain.Entities;

namespace GMB.BLL.Contracts
{
    /// <summary>
    /// Provides operations for managing room entities and room assignments.
    /// </summary>
    public interface IRoomService
    {
        /// <summary>
        /// Retrieves all rooms.
        /// </summary>
        /// <returns>A collection of all rooms.</returns>
        Task<IEnumerable<Room>> GetAllRoomsAsync();

        /// <summary>
        /// Retrieves a room by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the room.</param>
        /// <returns>The room if found; otherwise, null.</returns>
        Task<Room?> GetRoomByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a room by its unique room code.
        /// </summary>
        /// <param name="code">The room code.</param>
        /// <returns>The room if found; otherwise, null.</returns>
        Task<Room?> GetRoomByCodeAsync(string code);

        /// <summary>
        /// Adds a new room.
        /// </summary>
        /// <param name="room">The room to add.</param>
        Task AddRoomAsync(Room room);

        /// <summary>
        /// Updates an existing room.
        /// </summary>
        /// <param name="room">The room with updated information.</param>
        Task UpdateRoomAsync(Room room);

        /// <summary>
        /// Deletes a room by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the room to delete.</param>
        Task DeleteRoomAsync(Guid id);

        /// <summary>
        /// Retrieves all active rooms.
        /// </summary>
        /// <returns>A collection of active rooms.</returns>
        Task<IEnumerable<Room>> GetActiveRoomsAsync();

        /// <summary>
        /// Retrieves the room that the specified client is in.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The room if the client is found in one; otherwise, null.</returns>
        Task<Room?> GetRoomByClientAsync(Client client);

        /// <summary>
        /// Assigns a player to a room. If no available room exists, a new room is created.
        /// </summary>
        /// <param name="client">The client to assign.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in a room.</param>
        /// <returns>The room to which the client was assigned, or null if the room is full.</returns>
        Task<Room?> AssignPlayerToRoomAsync(Client client, int maxRoomSize);

        /// <summary>
        /// Allows a client to join a room by its code if there is available space.
        /// </summary>
        /// <param name="code">The room code.</param>
        /// <param name="client">The client attempting to join.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>The room if the client successfully joins; otherwise, null.</returns>
        Task<Room?> JoinRoomByCodeAsync(string code, Client client, int maxRoomSize);
    }
}

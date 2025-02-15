using GMB.Domain.Entities;

namespace GMB.BLL.Contracts
{
    /// <summary>
    /// Defines operations for managing room entities and handling room assignments.
    /// This includes creating, retrieving, updating, and deleting rooms,
    /// as well as handling room join operations (both by code and random assignment).
    /// </summary>
    public interface IRoomService
    {
        /// <summary>
        /// Retrieves all rooms from the data store.
        /// </summary>
        /// <returns>A collection of all <see cref="Room"/> objects.</returns>
        Task<IEnumerable<Room>> GetAllRoomsAsync();

        /// <summary>
        /// Retrieves a room by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the room.</param>
        /// <returns>The <see cref="Room"/> object if found; otherwise, null.</returns>
        Task<Room?> GetRoomByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a room by its room code.
        /// </summary>
        /// <param name="code">The room code.</param>
        /// <returns>The <see cref="Room"/> object if found; otherwise, null.</returns>
        Task<Room?> GetRoomByCodeAsync(string code);

        /// <summary>
        /// Updates an existing room with new information.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> object containing updated data.</param>
        Task UpdateRoomAsync(Room room);

        /// <summary>
        /// Deletes a room from the data store using its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the room to delete.</param>
        Task DeleteRoomAsync(Guid id);

        /// <summary>
        /// Retrieves all active rooms (i.e. rooms that are currently marked as active).
        /// </summary>
        /// <returns>A collection of active <see cref="Room"/> objects.</returns>
        Task<IEnumerable<Room>> GetActiveRoomsAsync();

        /// <summary>
        /// Retrieves the room in which the specified client is currently present.
        /// </summary>
        /// <param name="client">The <see cref="Client"/> object representing the player.</param>
        /// <returns>The <see cref="Room"/> if the client is in one; otherwise, null.</returns>
        Task<Room?> GetRoomByClientAsync(Client client);

        /// <summary>
        /// Allows a client to join a room identified by its code, if the room exists and has available space.
        /// </summary>
        /// <param name="code">The room code to join.</param>
        /// <param name="client">The <see cref="Client"/> object representing the player attempting to join.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>
        /// The <see cref="Room"/> object if the client successfully joins; 
        /// otherwise, null (e.g., if the room is full or inactive).
        /// </returns>
        Task<Room?> JoinRoomByCodeAsync(string code, Client client, int maxRoomSize);

        /// <summary>
        /// Retrieves an active non-private room that has available space for the specified client.
        /// If no such room exists, a new room is created.
        /// </summary>
        /// <param name="client">The <see cref="Client"/> object representing the player.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>
        /// An active non-private <see cref="Room"/> if available or successfully created; 
        /// otherwise, null if the room is full.
        /// </returns>
        Task<Room?> GetActiveNonPrivateRoom(Client client, int maxRoomSize);

        /// <summary>
        /// Creates a new private room for a client.
        /// </summary>
        /// <param name="client">The <see cref="Client"/> object for whom the private room is created.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>
        /// The newly created private <see cref="Room"/>; 
        /// otherwise, null if the room could not be created.
        /// </returns>
        Task<Room?> CreatePrivateRoomAsync(Client client, int maxRoomSize);
    }
}

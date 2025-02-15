using GMB.BLL.Contracts;                            // Business logic interfaces
using GMB.Domain.Entities;                          // Domain entities (e.g., Room, Client)
using GMB.Domain.Repositories.Interfaces;           // Repository interfaces for data access
using Microsoft.Extensions.Logging;                 // Logging interfaces

namespace GMB.BLL.Services
{
    /// <summary>
    /// Provides business logic for managing game rooms.
    /// This service handles operations related to Room entities such as retrieval, creation,
    /// updating, deletion, and assigning clients to rooms.
    /// </summary>
    public class RoomService : IRoomService
    {
        // Repository for accessing room data.
        private readonly IRoomRepository _roomRepository;
        // Repository for accessing client data.
        private readonly IClientRepository _clientRepository;
        // Logger for logging events and errors.
        private readonly ILogger<RoomService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomService"/> class.
        /// </summary>
        /// <param name="roomRepository">Repository for room data.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="clientRepository">Repository for client data.</param>
        public RoomService(IRoomRepository roomRepository, ILogger<RoomService> logger, IClientRepository clientRepository)
        {
            _roomRepository = roomRepository;
            _logger = logger;
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Retrieves all rooms asynchronously.
        /// </summary>
        /// <returns>A collection of all Room entities.</returns>
        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        /// <summary>
        /// Retrieves a room by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the room.</param>
        /// <returns>The Room entity if found; otherwise, null.</returns>
        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Retrieves a room by its code asynchronously.
        /// </summary>
        /// <param name="code">The room code.</param>
        /// <returns>The Room entity if found; otherwise, null.</returns>
        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            var room = await _roomRepository.GetRoomByCodeAsync(code);
            return room;
        }

        /// <summary>
        /// Updates an existing room in the data store asynchronously.
        /// </summary>
        /// <param name="room">The Room entity with updated values.</param>
        public async Task UpdateRoomAsync(Room room)
        {
            await _roomRepository.UpdateAsync(room);
        }

        /// <summary>
        /// Deletes a room by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the room to delete.</param>
        public async Task DeleteRoomAsync(Guid id)
        {
            await _roomRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Retrieves all active rooms (rooms that are currently marked as active) asynchronously.
        /// </summary>
        /// <returns>A collection of active Room entities.</returns>
        public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
        {
            // Filter and return only rooms that are marked as active.
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Where(r => r.IsActive);
        }

        /// <summary>
        /// Creates a new private room and assigns the given client as its initial member.
        /// </summary>
        /// <param name="client">The client who will own the private room.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>The newly created Room entity.</returns>
        public async Task<Room?> CreatePrivateRoomAsync(Client client, int maxRoomSize)
        {
            var newRoom = new Room
            {
                Id = Guid.NewGuid(),
                Code = GenerateUniqueRoomCode(),
                IsPrivate = true,
                MaxPlayers = maxRoomSize,
                Clients = new List<Client> { client }
            };

            await _roomRepository.AddAsync(newRoom);
            return newRoom;
        }

        /// <summary>
        /// Retrieves an active non-private room with available capacity for the given client.
        /// If no such room exists, a new room is created.
        /// </summary>
        /// <param name="client">The client to assign to a room.</param>
        /// <param name="maxRoomSize">The maximum allowed number of players in a room.</param>
        /// <returns>
        /// The Room entity the client was assigned to, or null if no room is available (i.e. room is full).
        /// </returns>
        public async Task<Room?> GetActiveNonPrivateRoom(Client client, int maxRoomSize)
        {
            // Get active non-private rooms with available space.
            var availableRooms = (await _roomRepository.GetActiveRoomsAsync(maxRoomSize, isPrivate: false)).ToList();
            var availableRoom = availableRooms.FirstOrDefault();

            if (availableRoom == null)
            {
                // Create a new room if no active room is available.
                availableRoom = new Room
                {
                    Id = Guid.NewGuid(),
                    Code = GenerateUniqueRoomCode(),
                    MaxPlayers = maxRoomSize,
                    Clients = new List<Client> { client }
                };
                await _roomRepository.AddAsync(availableRoom);
            }
            else
            {
                // Retrieve the current room from the data store.
                var updatedRoom = await _roomRepository.GetByIdAsync(availableRoom.Id);
                // If there is space available in the room, add the client.
                if (updatedRoom?.Clients.Count < maxRoomSize)
                {
                    updatedRoom.Clients.Add(client);
                    await _roomRepository.UpdateAsync(updatedRoom);
                }
                else
                {
                    return null; // The room is full.
                }
            }

            return availableRoom;
        }

        /// <summary>
        /// Allows a client to join a room using a specific room code if there is available space.
        /// </summary>
        /// <param name="code">The room code to join.</param>
        /// <param name="client">The client attempting to join.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>
        /// The Room entity if the client successfully joined; otherwise, null.
        /// </returns>
        public async Task<Room?> JoinRoomByCodeAsync(string code, Client client, int maxRoomSize)
        {
            var room = await GetRoomByCodeAsync(code);
            if (room != null && room.Clients.Count < maxRoomSize)
            {
                // Add the client to the room and update the room.
                room.Clients.Add(client);
                await UpdateRoomAsync(room);
                return room;
            }
            return null; // The room was not found or is already full.
        }

        /// <summary>
        /// Retrieves the room that the specified client is currently in.
        /// </summary>
        /// <param name="client">The client to look up.</param>
        /// <returns>The Room entity if the client is in a room; otherwise, null.</returns>
        public async Task<Room?> GetRoomByClientAsync(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client cannot be null");
            }

            var room = await _roomRepository.GetRoomByClientAsync(client);

            if (room == null)
            {
                _logger.LogWarning("Client {ClientId} was not found in any room", client.Id);
            }

            return room;
        }

        /// <summary>
        /// Generates a unique room code consisting of 6 uppercase characters.
        /// </summary>
        /// <returns>A unique room code string.</returns>
        private string GenerateUniqueRoomCode()
        {
            // Generate a GUID, take the first 6 characters, and convert to uppercase.
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }
    }
}

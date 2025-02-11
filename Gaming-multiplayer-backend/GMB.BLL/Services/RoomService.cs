using GMB.BLL.Contracts;                            // Business logic interfaces
using GMB.Domain.Entities;                          // Domain entities (e.g., Room, Client)
using GMB.Domain.Repositories.Interfaces;           // Repository interfaces for data access
using Microsoft.Extensions.Logging;                 // For logging

namespace GMB.BLL.Services
{
    // Implementation of the room-related business logic.
    // This service is responsible for handling operations related to Room entities,
    // such as retrieving, creating, updating, and deleting rooms, as well as assigning players to rooms.
    public class RoomService : IRoomService
    {
        // Repository for accessing room data.
        private readonly IRoomRepository _roomRepository;
        // Repository for accessing client data.
        private readonly IClientRepository _clientRepository;
        // Logger to record events and errors.
        private readonly ILogger<RoomService> _logger;

        // Constructor that uses dependency injection to receive repositories and logger.
        public RoomService(IRoomRepository roomRepository, ILogger<RoomService> logger, IClientRepository clientRepository)
        {
            _roomRepository = roomRepository;
            _logger = logger;
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Retrieves all rooms from the data store asynchronously.
        /// </summary>
        /// <returns>A collection of all Room objects.</returns>
        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        /// <summary>
        /// Retrieves a room by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the room.</param>
        /// <returns>The Room object if found; otherwise, null.</returns>
        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Retrieves a room by its code asynchronously.
        /// </summary>
        /// <param name="code">The room code.</param>
        /// <returns>The Room object if found; otherwise, null.</returns>
        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            var room = await _roomRepository.GetRoomByCodeAsync(code);
            return room;
        }

        /// <summary>
        /// Adds a new room to the data store asynchronously.
        /// </summary>
        /// <param name="room">The Room object to add.</param>
        public async Task AddRoomAsync(Room room)
        {
            await _roomRepository.AddAsync(room);
        }

        /// <summary>
        /// Updates an existing room in the data store asynchronously.
        /// </summary>
        /// <param name="room">The Room object with updated information.</param>
        public async Task UpdateRoomAsync(Room room)
        {
            await _roomRepository.UpdateAsync(room);
        }

        /// <summary>
        /// Deletes a room from the data store asynchronously by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the room to delete.</param>
        public async Task DeleteRoomAsync(Guid id)
        {
            await _roomRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Retrieves all active rooms (rooms that are currently marked as active) asynchronously.
        /// </summary>
        /// <returns>A collection of active Room objects.</returns>
        public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
        {
            // Filter the list of all rooms to only include those that are active.
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Where(r => r.IsActive);
        }

        /// <summary>
        /// Assigns a player (client) to an active room with available space.
        /// If no available room exists, a new room is created.
        /// </summary>
        /// <param name="client">The Client object representing the player.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in a room.</param>
        /// <returns>The Room object the player was assigned to, or null if the room is full.</returns>
        public async Task<Room?> AssignPlayerToRoomAsync(Client client, int maxRoomSize)
        {
            // Get a list of active rooms that have fewer players than the maximum allowed.
            var availableRooms = (await _roomRepository.GetActiveRoomsAsync(maxRoomSize)).ToList();
            // Select the first available room.
            var availableRoom = availableRooms.FirstOrDefault();

            if (availableRoom == null)
            {
                // If no available room exists, create a new room.
                availableRoom = new Room
                {
                    Id = Guid.NewGuid(),
                    Code = GenerateUniqueRoomCode(),
                    MaxPlayers = maxRoomSize,
                    Clients = new List<Client>()
                };

                // Try to retrieve the client from the repository.
                var existingClient = await _clientRepository.GetByIdAsync(client.Id);
                if (existingClient == null)
                {
                    // If the client is not found, add the new client instance.
                    availableRoom.Clients.Add(client);
                }
                else
                {
                    // If the client exists, add the existing instance.
                    availableRoom.Clients.Add(existingClient);
                }

                // Save the newly created room.
                await _roomRepository.AddAsync(availableRoom);
            }
            else
            {
                // If an available room exists, update the room by adding the client if there is space.
                var updatedRoom = await _roomRepository.GetByIdAsync(availableRoom.Id);
                if (updatedRoom?.Clients.Count < maxRoomSize)
                {
                    var existingClient = await _clientRepository.GetByIdAsync(client.Id);
                    if (existingClient == null)
                    {
                        updatedRoom.Clients.Add(client);
                    }
                    else
                    {
                        updatedRoom.Clients.Add(existingClient);
                    }

                    await _roomRepository.UpdateAsync(updatedRoom);
                }
                else
                {
                    // If the room is already full, return null.
                    return null;
                }
            }

            return availableRoom;
        }

        /// <summary>
        /// Joins a room by a specific code if there is available space.
        /// </summary>
        /// <param name="code">The room code.</param>
        /// <param name="client">The Client object representing the player.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        /// <returns>The Room object if successfully joined; otherwise, null.</returns>
        public async Task<Room?> JoinRoomByCodeAsync(string code, Client client, int maxRoomSize)
        {
            var room = await GetRoomByCodeAsync(code);
            if (room != null && room.Clients.Count < maxRoomSize)
            {
                // Add the client to the room and update it.
                room.Clients.Add(client);
                await UpdateRoomAsync(room);
                return room;
            }

            return null; // The room is either not found or full.
        }

        /// <summary>
        /// Retrieves the room that the specified client is currently in.
        /// </summary>
        /// <param name="client">The Client object.</param>
        /// <returns>The Room object if the client is in a room; otherwise, null.</returns>
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
        /// Generates a unique room code.
        /// </summary>
        /// <returns>A unique, uppercase 6-character room code.</returns>
        private string GenerateUniqueRoomCode()
        {
            // Uses a GUID to generate a unique string, takes the first 6 characters, and converts them to uppercase.
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }
    }
}

using GMB.BLL.Contracts;                         // Interfaces for business logic services.
using GMB.Domain.Repositories.Interfaces;        // Interfaces for repositories that interact with data storage.

namespace GMB.BLL.Services
{
    // Implementation of the client-related business logic.
    // This service acts as an intermediary between the controllers and the data repositories.
    public class ClientService : IClientService
    {
        // Repository for accessing Client data from the database.
        private readonly IClientRepository _clientRepository;

        // Constructor with dependency injection.
        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Retrieves all clients asynchronously.
        /// </summary>
        /// <returns>An enumerable of all Client objects.</returns>
        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            // Delegates the call to the repository.
            return await _clientRepository.GetAllAsync();
        }

        /// <summary>
        /// Retrieves a client by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the client.</param>
        /// <returns>A Client object or null if not found.</returns>
        public async Task<Client?> GetClientByIdAsync(Guid id)
        {
            return await _clientRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Adds a new client to the data store.
        /// </summary>
        /// <param name="client">The Client object to add.</param>
        public async Task AddClientAsync(Client client)
        {
            await _clientRepository.AddAsync(client);
        }

        /// <summary>
        /// Updates an existing client's information.
        /// </summary>
        /// <param name="client">The Client object with updated information.</param>
        public async Task UpdateClientAsync(Client client)
        {
            await _clientRepository.UpdateAsync(client);
        }

        /// <summary>
        /// Deletes a client from the data store by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the client to delete.</param>
        public async Task DeleteClientAsync(Guid id)
        {
            await _clientRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Retrieves a client based on the SignalR connection identifier.
        /// </summary>
        /// <param name="connectionId">The connection ID provided by SignalR.</param>
        /// <returns>A Client object or null if not found.</returns>
        public async Task<Client?> GetClientByConnectionIdAsync(string connectionId)
        {
            return await _clientRepository.GetByConnectionIdAsync(connectionId);
        }

        /// <summary>
        /// Retrieves all clients that are currently active.
        /// </summary>
        /// <returns>An enumerable of active Client objects.</returns>
        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
        {
            return await _clientRepository.GetActiveClientsAsync();
        }

        /// <summary>
        /// Retrieves all clients in a specific room.
        /// </summary>
        /// <param name="roomCode">The code identifying the room.</param>
        /// <returns>An enumerable of Client objects in the room.</returns>
        public async Task<IEnumerable<Client>> GetClientsInRoom(string roomCode)
        {
            return await _clientRepository.GetClientsInRoom(roomCode);
        }

        /// <summary>
        /// Ensures a client exists for the given connection ID.
        /// If a client with the specified connection ID does not exist, it creates one.
        /// </summary>
        /// <param name="connectionId">The SignalR connection ID.</param>
        /// <returns>The existing or newly created Client object.</returns>
        public async Task<Client> EnsureClientExistsAsync(string connectionId)
        {
            // Try to get the client from the repository by its connection ID.
            var client = await _clientRepository.GetByConnectionIdAsync(connectionId);

            // If not found, create a new Client instance with a new GUID and default name "Guest".
            if (client == null)
            {
                client = new Client
                {
                    Id = Guid.NewGuid(),
                    ConnectionId = connectionId,
                    Name = "Guest"
                };

                // Save the new client to the repository.
                await _clientRepository.AddAsync(client);
            }

            return client;
        }

        /// <summary>
        /// Retrieves the last client that joined the specified room.
        /// </summary>
        /// <param name="roomCode">The code of the room.</param>
        /// <returns>The last joined Client object, or null if no client is found.</returns>
        public async Task<Client?> GetLastJoinedClientAsync(string roomCode)
        {
            return await _clientRepository.GetLastJoinedClientAsync(roomCode);
        }
    }
}

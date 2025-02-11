using GMB.Domain.Entities;

namespace GMB.BLL.Contracts
{
    /// <summary>
    /// Provides operations for managing client entities.
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Retrieves all clients.
        /// </summary>
        /// <returns>A collection of all clients.</returns>
        Task<IEnumerable<Client>> GetAllClientsAsync();

        /// <summary>
        /// Retrieves a client by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the client.</param>
        /// <returns>The client if found; otherwise, null.</returns>
        Task<Client?> GetClientByIdAsync(Guid id);

        /// <summary>
        /// Adds a new client.
        /// </summary>
        /// <param name="client">The client to add.</param>
        Task AddClientAsync(Client client);

        /// <summary>
        /// Updates an existing client.
        /// </summary>
        /// <param name="client">The client with updated information.</param>
        Task UpdateClientAsync(Client client);

        /// <summary>
        /// Deletes a client by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the client to delete.</param>
        Task DeleteClientAsync(Guid id);

        /// <summary>
        /// Retrieves a client by its SignalR connection identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <returns>The client if found; otherwise, null.</returns>
        Task<Client?> GetClientByConnectionIdAsync(string connectionId);

        /// <summary>
        /// Retrieves all clients that are currently in a specified room.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <returns>A collection of clients in the specified room.</returns>
        Task<IEnumerable<Client>> GetClientsInRoom(string roomCode);

        /// <summary>
        /// Ensures a client with the specified connection ID exists.
        /// If no client is found, a new client is created.
        /// </summary>
        /// <param name="connectionId">The SignalR connection identifier.</param>
        /// <returns>The existing or newly created client.</returns>
        Task<Client> EnsureClientExistsAsync(string connectionId);

        /// <summary>
        /// Retrieves the last client that joined the specified room.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <returns>The last joined client if found; otherwise, null.</returns>
        Task<Client?> GetLastJoinedClientAsync(string roomCode);
    }
}

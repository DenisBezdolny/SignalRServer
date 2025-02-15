using GMB.Domain.Entities;
using GMB.Domain.Repositories.Interfaces;
using GMB.Domain.Repositories.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GMB.Domain.Repositories.Repositories
{
    /// <summary>
    /// Implements the <see cref="IClientRepository"/> interface to provide data access methods for <see cref="Client"/> entities.
    /// Inherits from the generic <see cref="Repository{Client}"/> class.
    /// </summary>
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly GMB_DbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRepository"/> class using the specified database context.
        /// </summary>
        /// <param name="dbContext">The database context used for accessing Client entities.</param>
        public ClientRepository(GMB_DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a client by connection ID without tracking changes.
        /// </summary>
        /// <param name="connectionId">The connection identifier of the client.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the <see cref="Client"/> if found; otherwise, null.
        /// </returns>
        public async Task<Client?> GetByConnectionIdAsNoTrackingAsync(string connectionId)
        {
            // AsNoTracking improves performance when the entity is read-only.
            return await _dbContext.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }

        /// <summary>
        /// Retrieves a client by connection ID with change tracking enabled.
        /// </summary>
        /// <param name="connectionId">The connection identifier of the client.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the <see cref="Client"/> if found; otherwise, null.
        /// </returns>
        public async Task<Client?> GetByConnectionIdAsync(string connectionId)
        {
            // Returns the client entity that matches the given connection ID.
            return await _dbContext.Clients
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }

        /// <summary>
        /// Retrieves all active clients.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a collection of active <see cref="Client"/> entities.
        /// </returns>
        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
        {
            // A client is considered active if its ConnectionId is not null or empty.
            return await _dbContext.Clients
                .Where(c => !string.IsNullOrEmpty(c.ConnectionId))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all clients that are in a specified room.
        /// </summary>
        /// <param name="roomCode">The code of the room.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a collection of <see cref="Client"/> entities in the room.
        /// </returns>
        public async Task<IEnumerable<Client>> GetClientsInRoom(string roomCode)
        {
            // Filters the clients whose associated room has the specified room code.
            return await _dbContext.Clients
                .Where(c => c.Room.Code == roomCode)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the last joined client in a specified room based on the joining time.
        /// </summary>
        /// <param name="roomCode">The code of the room.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the last joined <see cref="Client"/> in the room if found; otherwise, null.
        /// </returns>
        public async Task<Client?> GetLastJoinedClientAsync(string roomCode)
        {
            // Orders the clients in the room by JoinedAt in descending order (latest first)
            // and returns the first one, without tracking changes.
            return await _dbContext.Clients
                .Where(c => c.Room.Code == roomCode)
                .OrderByDescending(c => c.JoinedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}

using GMB.Domain.Entities;
using GMB.Domain.Repositories.Interfaces;
using GMB.Domain.Repositories.Reposytories; // (Note: Check the namespace spelling if needed)
using Microsoft.EntityFrameworkCore;

namespace GMB.Domain.Repositories.Repositories
{
    /// <summary>
    /// Implements data access logic for Room entities.
    /// Inherits from the generic Repository base class and implements the IRoomRepository interface.
    /// </summary>
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        private readonly GMB_DbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomRepository"/> class with the specified database context.
        /// </summary>
        /// <param name="dbContext">The Entity Framework database context used for data operations.</param>
        public RoomRepository(GMB_DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a room by its unique code.
        /// The query includes the associated clients for the room.
        /// </summary>
        /// <param name="code">The unique code identifying the room.</param>
        /// <returns>
        /// A <see cref="Room"/> entity if found; otherwise, null.
        /// </returns>
        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            return await _dbContext.Rooms
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Code == code);
        }

        /// <summary>
        /// Retrieves all active rooms that have fewer clients than the specified maximum.
        /// The query includes the associated clients and orders the rooms by the current client count in ascending order.
        /// </summary>
        /// <param name="maxPlayers">The maximum allowed number of clients (players) per room.</param>
        /// <returns>
        /// A collection of active <see cref="Room"/> entities.
        /// </returns>
        public async Task<IEnumerable<Room>> GetActiveRoomsAsync(int maxPlayers)
        {
            return await _dbContext.Rooms
                .Include(r => r.Clients)
                .Where(r => r.Clients.Count < maxPlayers)
                .OrderBy(r => r.Clients.Count)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the room in which the specified client is currently present.
        /// The query includes the list of clients for each room.
        /// </summary>
        /// <param name="client">The client whose room membership is to be determined.</param>
        /// <returns>
        /// A <see cref="Room"/> entity that contains the client; otherwise, null.
        /// </returns>
        public async Task<Room?> GetRoomByClientAsync(Client client)
        {
            return await _dbContext.Rooms
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Clients.Any(c => c.Id == client.Id));
        }
    }
}

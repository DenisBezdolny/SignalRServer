using GMB.Domain.Entities;
using GMB.Domain.Repositories.Interfaces;
using GMB.Domain.Repositories.Reposytories;
using Microsoft.EntityFrameworkCore;

namespace GMB.Domain.Repositories.Repositories
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        private readonly GMB_DbContext _dbContext;

        public RoomRepository(GMB_DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            return await _dbContext.Rooms
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<IEnumerable<Room>> GetActiveRoomsAsync(int maxPlayers)
        {
            return await _dbContext.Rooms
                .Include(r => r.Clients) 
                .Where(r => r.Clients.Count < maxPlayers)
                .OrderBy(r => r.Clients.Count)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomByClientAsync(Client client)
        {
            return await _dbContext.Rooms
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Clients.Any(c => c.Id == client.Id));
        }

    }
}

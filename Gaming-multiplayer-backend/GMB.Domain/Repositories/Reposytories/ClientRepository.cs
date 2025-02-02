using GMB.Domain.Entities;
using GMB.Domain.Repositories.Interfaces;
using GMB.Domain.Repositories.Reposytories;
using Microsoft.EntityFrameworkCore;

namespace GMB.Domain.Repositories.Repositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly GMB_DbContext _dbContext;

        public ClientRepository(GMB_DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Client?> GetByConnectionIdAsync(string connectionId)
        {
            return await _dbContext.Clients
                .AsNoTracking() 
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }

        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
        {
            return await _dbContext.Clients
                .Where(c => !string.IsNullOrEmpty(c.ConnectionId))
                .ToListAsync();
        }
    }
}

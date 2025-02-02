using GMB.BLL.Contracts;
using GMB.Domain.Entities;
using GMB.Domain.Repositories.Interfaces;

namespace GMB.BLL.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _clientRepository.GetAllAsync();
        }

        public async Task<Client?> GetClientByIdAsync(Guid id)
        {
            return await _clientRepository.GetByIdAsync(id);
        }

        public async Task AddClientAsync(Client client)
        {
            await _clientRepository.AddAsync(client);
        }

        public async Task UpdateClientAsync(Client client)
        {
            await _clientRepository.UpdateAsync(client);
        }

        public async Task DeleteClientAsync(Guid id)
        {
            await _clientRepository.DeleteAsync(id);
        }

        public async Task<Client?> GetClientByConnectionIdAsync(string connectionId)
        {
            return await _clientRepository.GetByConnectionIdAsync(connectionId);
        }

        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
        {
            return await _clientRepository.GetActiveClientsAsync();
        }
    }
}

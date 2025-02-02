using GMB.Domain.Entities;


namespace GMB.BLL.Contracts
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client?> GetClientByIdAsync(Guid id);
        Task AddClientAsync(Client client);
        Task UpdateClientAsync(Client client);
        Task DeleteClientAsync(Guid id);
        Task<Client?> GetClientByConnectionIdAsync(string connectionId);
    }
}

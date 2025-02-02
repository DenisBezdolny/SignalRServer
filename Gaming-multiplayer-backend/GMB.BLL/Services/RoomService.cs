using GMB.BLL.Contracts;
using GMB.Domain.Entities;
using GMB.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GMB.BLL.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ILogger<RoomService> _logger;

        public RoomService(IRoomRepository roomRepository, ILogger<RoomService> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            var room = await _roomRepository.GetRoomByCodeAsync(code);
            return room;
        }

        public async Task AddRoomAsync(Room room)
        {
            await _roomRepository.AddAsync(room);
        }

        public async Task UpdateRoomAsync(Room room)
        {
            await _roomRepository.UpdateAsync(room);
        }

        public async Task DeleteRoomAsync(Guid id)
        {
            await _roomRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Where(r => r.IsActive);
        }

        public async Task<Room?> AssignPlayerToRoomAsync(Client client, int maxRoomSize)
        {
            var availableRooms = (await _roomRepository.GetActiveRoomsAsync(maxRoomSize)).ToList();
            var availableRoom = availableRooms.FirstOrDefault();

            if (availableRoom == null)
            {
                availableRoom = new Room
                {
                    Id = Guid.NewGuid(),
                    Code = GenerateUniqueRoomCode(),
                    MaxPlayers = maxRoomSize,
                    Clients = new List<Client> { client }
                };
                await AddRoomAsync(availableRoom);
            }
            else
            {
                // Повторная проверка актуальности комнаты перед добавлением
                var updatedRoom = await _roomRepository.GetByIdAsync(availableRoom.Id);
                if (updatedRoom?.Clients.Count < maxRoomSize)
                {
                    updatedRoom.Clients.Add(client);
                    await UpdateRoomAsync(updatedRoom);
                }
                else
                {
                    // Возвращаем ошибку, если комната переполнена
                    return null;
                }
            }

            return availableRoom;
        }

        public async Task<Room?> JoinRoomByCodeAsync(string code, Client client, int maxRoomSize)
        {
            var room = await GetRoomByCodeAsync(code);
            if (room != null && room.Clients.Count < maxRoomSize)
            {
                room.Clients.Add(client);
                await UpdateRoomAsync(room);
                return room;
            }

            return null; // Комната недоступна
        }
        public async Task<Room?> GetRoomByClientAsync(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Клиент не может быть null");
            }

            var room = await _roomRepository.GetRoomByClientAsync(client);

            if (room == null)
            {
                _logger.LogWarning("Клиент {ClientId} не найден ни в одной комнате", client.Id);
            }

            return room;
        }

        // Генерация уникального кода комнаты
        private string GenerateUniqueRoomCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }

    }
}

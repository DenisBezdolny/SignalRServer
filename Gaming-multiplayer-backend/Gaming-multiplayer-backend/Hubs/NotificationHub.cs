using Microsoft.AspNetCore.SignalR;
using GMB.BLL.Contracts;
using GMB.Domain.Entities;

namespace Gaming_multiplayer_backend.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly IClientService _clientService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(IRoomService roomService, IClientService clientService, ILogger<NotificationHub> logger)
        {
            _roomService = roomService;
            _clientService = clientService;
            _logger = logger;
        }

        public async Task JoinRoom(string roomCode, int maxRoomSize)
        {
            
            var connectionId = Context.ConnectionId;

            // Попробуем найти клиента по ConnectionId
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);

            // Если клиента нет в базе данных, создаём нового
            if (client == null)
            {
                client = new Client { Id = Guid.NewGuid(), ConnectionId = connectionId };
                await _clientService.AddClientAsync(client);  // Сохраняем нового клиента в БД
            }

            // Если имя пустое или начинается с "Guest", присваиваем имя по умолчанию
            if (string.IsNullOrEmpty(client.Name) || client.Name.StartsWith("Guest"))
            {
                client.Name = $"Guest";
            }

            Room room = await _roomService.JoinRoomByCodeAsync(roomCode, client, maxRoomSize);
            if (room != null)
            {
                await Groups.AddToGroupAsync(connectionId, roomCode);
                await Clients.Group(roomCode)
                            .SendAsync("ReceiveMessage", "System", $"{client.Name} joined random room {roomCode}");
                await Clients.Group(roomCode)
                            .SendAsync("UpdateParticipants", room.Clients.Select(c => c.Name).ToList());
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome to room {roomCode}.");
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Unable to join the room. Room is full or inactive.");
            }
        }

        public async Task JoinRandomRoom(int maxRoomSize)
        {
            var connectionId = Context.ConnectionId;

            // Попробуем найти клиента по ConnectionId
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);

            // Если клиента нет в базе данных, создаём нового
            if (client == null)
            {
                client = new Client { Id = Guid.NewGuid(), ConnectionId = connectionId };
                await _clientService.AddClientAsync(client);  // Сохраняем нового клиента в БД
            }

            // Если имя пустое или начинается с "Guest", присваиваем имя по умолчанию
            if (string.IsNullOrEmpty(client.Name) || client.Name.StartsWith("Guest"))
            {
                client.Name = $"Guest";
            }

            var assignedRoom = await _roomService.AssignPlayerToRoomAsync(client, maxRoomSize);
            if (assignedRoom != null)
            {
                await Groups.AddToGroupAsync(connectionId, assignedRoom.Code);
                await Clients.Group(assignedRoom.Code)
                            .SendAsync("ReceiveMessage", "System", $"{client.Name} joined random room {assignedRoom.Code}");
                await Clients.Group(assignedRoom.Code)
                            .SendAsync("UpdateParticipants", assignedRoom.Clients.Select(c => c.Name).ToList());
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome to room {assignedRoom.Code}.");
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Unable to join a random room. No active rooms available.");
            }
        }

        public async Task LeaveRoom(string roomCode)
        {
            var connectionId = Context.ConnectionId;

            // Удаляем клиента из комнаты
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);
            _logger.LogInformation("Клиент {ConnectionId} покинул комнату {roomCode}", Context.ConnectionId, roomCode);
            if (client != null)
            {
                var room = await _roomService.GetRoomByCodeAsync(roomCode);
                if (room != null)
                {
                    room.Clients.Remove(client);
                    await _roomService.UpdateRoomAsync(room);
                    await Groups.RemoveFromGroupAsync(connectionId, room.Code);
                    await Clients.Group(room.Code).SendAsync("ReceiveMessage", "System", $"{connectionId} left room {room.Code}");
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Notify", "Connected to the server.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);

            if (client != null)
            {
                var room = await _roomService.GetRoomByClientAsync(client);
                if (room != null)
                {
                    room.Clients.Remove(client); // Удаляем клиента из комнаты
                    await _roomService.UpdateRoomAsync(room); // Сохраняем изменения

                    await Clients.Group(room.Code).SendAsync("ReceiveMessage", "System", $"{client.Name} покинул комнату.");
                    await Groups.RemoveFromGroupAsync(connectionId, room.Code);
                }

                await _clientService.DeleteClientAsync(client.Id); // Удаляем клиента из БД
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToRoom(string roomCode, string message)
        {
            var connectionId = Context.ConnectionId;

            // Рассылка сообщения всем участникам комнаты
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", connectionId, message);
        }

        public async Task SendNATTraversalInfo(string roomCode, string publicIp, int publicPort)
        {
            var connectionId = Context.ConnectionId;

            // Обновление данных клиента
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);
            if (client != null)
            {
                client.PublicIp = publicIp;
                client.PublicPort = publicPort;
                await _clientService.UpdateClientAsync(client);

                // Рассылка информации о NAT-траверсинге другим участникам комнаты
                var room = await _roomService.GetRoomByCodeAsync(roomCode);
                if (room != null)
                {
                    await Clients.GroupExcept(roomCode, connectionId)
                        .SendAsync("ReceiveNATInfo", connectionId, publicIp, publicPort);
                }
            }
        }
    }
}

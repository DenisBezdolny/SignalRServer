using Microsoft.AspNetCore.SignalR;
using GMB.BLL.Contracts;
using System.Collections.Concurrent;

namespace Gaming_multiplayer_backend.Hubs
{
    /// <summary>
    /// SignalR hub for sending notifications, offers, answers, and ICE candidates between clients.
    /// This hub manages client connections, room join/leave events, and handles P2P signaling.
    /// </summary>
    public class NotificationHub : Hub
    {
        // Services used to manage rooms and clients.
        private readonly IRoomService _roomService;
        private readonly IClientService _clientService;
        private readonly ILogger<NotificationHub> _logger;
        private readonly IConfiguration _configuration;

        // Dictionaries to track connected clients and their connection times.
        private static readonly ConcurrentDictionary<string, string> ConnectedClients = new();

        // Constants for the STUN and TURN server addresses.
        // (Fetch from appsettings)
        private string StunServer => _configuration.GetValue<string>("WebRTC:StunServer");
        private string TurnServer => _configuration.GetValue<string>("WebRTC:TurnServer"); 

        /// <summary>
        /// Constructor for NotificationHub.
        /// </summary>
        /// <param name="roomService">Service to manage room-related operations.</param>
        /// <param name="clientService">Service to manage client-related operations.</param>
        /// <param name="logger">Logger instance for logging messages.</param>
        public NotificationHub(IRoomService roomService, IClientService clientService, ILogger<NotificationHub> logger, IConfiguration configuration)
        {
            _roomService = roomService;
            _clientService = clientService;
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Allows a client to join a room by a given room code.
        /// The client is added to the room group, and all room participants are notified.
        /// </summary>
        /// <param name="roomCode">The room code to join.</param>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        public async Task JoinRoom(string roomCode, int maxRoomSize)
        {
            string connectionId = Context.ConnectionId;

            // Ensure the client exists or create a new one.
            var client = await _clientService.EnsureClientExistsAsync(connectionId);

            // Attempt to join the room using the provided room code.
            var assignedRoom = await _roomService.JoinRoomByCodeAsync(roomCode, client, maxRoomSize);
            if (assignedRoom != null)
            {
                // Record the time the client joined.
                client.JoinedAt = DateTime.UtcNow;
                await _clientService.UpdateClientAsync(client);

                // Add the client to the SignalR group corresponding to the room.
                await Groups.AddToGroupAsync(connectionId, roomCode);

                // Log that the client has joined the room.
                _logger.LogInformation("Client {ClientId} ({ConnectionId}) joined room {RoomCode}", client.Id, connectionId, roomCode);

                // Notify all clients in the room that the client has joined.
                await Clients.Group(roomCode)
                            .SendAsync("ReceiveMessage", "System", $"{client.Name} joined random room {roomCode}");

                // Send updated participant list to all clients in the room.
                await Clients.Group(roomCode)
                            .SendAsync("UpdateRoomParticipants", assignedRoom.Clients.Select(c => new { id = c.ConnectionId, name = c.Name })
                            .ToList());

                // Send a welcome message to the caller.
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome to room {roomCode}.");
            }
            else
            {
                // If the room is full or inactive, log a warning and notify the caller.
                _logger.LogWarning("Client {ConnectionId} failed to join room {RoomCode}", connectionId, roomCode);
                await Clients.Caller.SendAsync("Error", "Unable to join the room. Room is full or inactive.");
            }
        }

        /// <summary>
        /// Assigns the client to a random room if possible.
        /// The client is added to the room and all room participants are updated.
        /// </summary>
        /// <param name="maxRoomSize">The maximum allowed room size.</param>
        public async Task JoinRandomRoom(int maxRoomSize)
        {

            string connectionId = Context.ConnectionId;

            // Ensure the client exists or create a new one.
            var client = await _clientService.EnsureClientExistsAsync(connectionId);

            // Attempt to assign the client to an active room.
            var assignedRoom = await _roomService.GetActiveNonPrivateRoom(client, maxRoomSize);
            if (assignedRoom != null)
            {
                // Record the join time.
                client.JoinedAt = DateTime.UtcNow;
                await _clientService.UpdateClientAsync(client);

                // Add the client to the room's SignalR group.
                await Groups.AddToGroupAsync(connectionId, assignedRoom.Code);

                // Notify the room that the client has joined.
                await Clients.Group(assignedRoom.Code)
                            .SendAsync("ReceiveMessage", "System", $"{client.Name} joined random room {assignedRoom.Code}");

                _logger.LogInformation("Sending UpdateRoomParticipants event for room {RoomCode}", assignedRoom.Code);

                // Update all clients in the room with the new list of participants.
                await Clients.Group(assignedRoom.Code)
                            .SendAsync("updateroomparticipants", assignedRoom.Clients.Select(c => new {
                                id = c.ConnectionId,
                                name = c.Name,
                                publicIp = c.PublicIp, // Public IP of the client
                                publicPort = c.PublicPort // Public port of the client
                            }).ToList());

                // Send a welcome message to the client.
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome to room {assignedRoom.Code}.");
            }
            else
            {
                // If no active room is available, send an error message to the client.
                await Clients.Caller.SendAsync("Error", "Unable to join a random room. No active rooms available.");
            }
        }

        /// <summary>
        /// Creates a new private room for the client.
        /// </summary>
        /// <param name="maxRoomSize">The maximum number of players allowed in the room.</param>
        public async Task CreatePrivateRoom(int maxRoomSize)
        {
            // Get the connection ID of the current client from the SignalR context.
            string connectionId = Context.ConnectionId;

            // Ensure that a client record exists for this connection.
            // If not, a new client is created with default values.
            var client = await _clientService.EnsureClientExistsAsync(connectionId);

            // Create a new private room for this client.
            // The CreatePrivateRoomAsync method returns the newly created room.
            var assignedRoom = await _roomService.CreatePrivateRoomAsync(client, maxRoomSize);

            if (assignedRoom != null)
            {
                // Record the time when the client joined the room.
                client.JoinedAt = DateTime.UtcNow;
                // Update the client's record in the data store.
                await _clientService.UpdateClientAsync(client);

                // Add the client to the SignalR group corresponding to the room code.
                await Groups.AddToGroupAsync(connectionId, assignedRoom.Code);

                // Notify all clients in the room that this client has joined,
                // by sending a system message to the room group.
                await Clients.Group(assignedRoom.Code)
                             .SendAsync("ReceiveMessage", "System", $"{client.Name} joined random room {assignedRoom.Code}");

                // Log the event that the client has joined the room.
                _logger.LogInformation("Sending UpdateRoomParticipants event for room {RoomCode}", assignedRoom.Code);

                // Update all clients in the room with the new list of participants.
                // The list includes each participant's connection ID, name, public IP, and public port.
                await Clients.Group(assignedRoom.Code)
                             .SendAsync("updateroomparticipants", assignedRoom.Clients.Select(c => new {
                                 id = c.ConnectionId,
                                 name = c.Name,
                                 publicIp = c.PublicIp, // Public IP of the client
                                 publicPort = c.PublicPort // Public port of the client
                             }).ToList());

                // Send a welcome message directly to the caller (the client who created the room).
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome to room {assignedRoom.Code}.");
            }
            else
            {
                // If for some reason a room could not be created or joined,
                // send an error message back to the caller.
                await Clients.Caller.SendAsync("Error", "Unable to join a random room. No active rooms available.");
            }
        }

        /// <summary>
        /// Sends a chat message to all clients in a specified room.
        /// </summary>
        /// <param name="roomCode">The room code where the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        public async Task SendMessageToRoom(string roomCode, string message)
        {
            // Retrieve the connectionId and client details.
            var connectionId = Context.ConnectionId;
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);

            // Broadcast the message to all clients in the room.
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", client.Name, message);
        }

        /// <summary>
        /// Removes the client from a room.
        /// The client is removed from the room's group and the remaining clients are notified.
        /// </summary>
        /// <param name="roomCode">The code of the room to leave.</param>
        public async Task LeaveRoom(string roomCode)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Client {ConnectionId} is leaving room {RoomCode}", connectionId, roomCode);

            // Get the client object using the connectionId.
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);
            if (client != null)
            {
                // Retrieve the room by its code.
                var room = await _roomService.GetRoomByCodeAsync(roomCode);
                if (room != null)
                {
                    // Remove the client from the room and update the room.
                    room.Clients.Remove(client);
                    await _roomService.UpdateRoomAsync(room);
                    // Remove the client from the SignalR group.
                    await Groups.RemoveFromGroupAsync(connectionId, room.Code);

                    // Notify all clients in the room that the client has left.
                    await Clients.Group(roomCode).SendAsync("ReceiveMessage", "System", $"{client.Name} left the room.");
                }
            }
        }

        /// <summary>
        /// Called when a new connection is established.
        /// Ensures the client exists and adds the client to the list of connected clients.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;

            // Ensure the client exists.
            var client = await _clientService.EnsureClientExistsAsync(connectionId);

            // Add the client to the local dictionary of connected clients.
            ConnectedClients.TryAdd(connectionId, client.Name);

            _logger.LogInformation("Client {ConnectionId} connected", connectionId);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a connection is terminated.
        /// Removes the client from the room (if any), logs the disconnection, and deletes the client record.
        /// </summary>
        /// <param name="exception">Optional exception information.</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Retrieve the client based on the connectionId.
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);
            if (client != null)
            {
                // If the client belongs to a room, remove it.
                var room = await _roomService.GetRoomByClientAsync(client);
                if (room != null)
                {
                    room.Clients.Remove(client);
                    await _roomService.UpdateRoomAsync(room);
                    _logger.LogWarning("[{Timestamp}] Client {ConnectionId} disconnected", DateTime.UtcNow, connectionId);
                    await Clients.Group(room.Code).SendAsync("ReceiveMessage", "System", $"{client.Name} left the room.");
                    await Groups.RemoveFromGroupAsync(connectionId, room.Code);
                }

                // Delete the client record.
                await _clientService.DeleteClientAsync(client.Id);
                _logger.LogWarning("[{Timestamp}] Client {ConnectionId} REMOVED", DateTime.UtcNow, connectionId);
                ConnectedClients.TryRemove(connectionId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Handles the client's request for STUN server information.
        /// Returns the configured STUN server to the caller.
        /// </summary>
        public async Task RequestSTUNInfo()
        {
            _logger.LogInformation("Client requested RequestSTUNInfo");
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Client {ConnectionId} requested STUN server", connectionId);

            await Clients.Caller.SendAsync("ReceiveSTUNServer", StunServer);
        }

        /// <summary>
        /// Processes NAT traversal information received from a client.
        /// Updates the client's public IP and port, then notifies other clients in the room.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <param name="publicIp">The client's public IP address.</param>
        /// <param name="publicPort">The client's public port.</param>
        public async Task SendNATTraversalInfo(string roomCode, string publicIp, int publicPort)
        {
            _logger.LogInformation("Client requested SendNATTraversalInfo");
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Client {ConnectionId} sent NAT info: {PublicIp}:{PublicPort}", connectionId, publicIp, publicPort);

            // Update the client's NAT information in the database.
            var client = await _clientService.GetClientByConnectionIdAsync(connectionId);
            if (client != null)
            {
                client.PublicIp = publicIp;
                client.PublicPort = publicPort;
                await _clientService.UpdateClientAsync(client);

                // Retrieve the room and notify all clients except the sender.
                var room = await _roomService.GetRoomByCodeAsync(roomCode);
                if (room != null)
                {
                    _logger.LogInformation("Server sending NATInfo via ReceiveNATInfo");
                    await Clients.GroupExcept(roomCode, connectionId)
                        .SendAsync("ReceiveNATInfo", connectionId, publicIp, publicPort);
                }
            }
        }

        /// <summary>
        /// Reports that a client failed to establish a P2P connection.
        /// Sends back the TURN server information to the caller.
        /// </summary>
        /// <param name="roomCode">The room code in which the P2P connection failed.</param>
        public async Task ReportP2PFailure(string roomCode)
        {
            _logger.LogInformation("Client requested ReportP2PFailure");
            var connectionId = Context.ConnectionId;
            _logger.LogWarning("Client {ConnectionId} failed to establish P2P connection in room {RoomCode}", connectionId, roomCode);

            await Clients.Caller.SendAsync("ReceiveTURNServer", TurnServer);
        }

        /// <summary>
        /// Sends an offer from the caller to a specified target client.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <param name="targetConnectionId">The target client's connection ID.</param>
        /// <param name="offer">The WebRTC offer.</param>
        public async Task SendOffer(string roomCode, string targetConnectionId, string offer)
        {
            string connectionId = Context.ConnectionId;
            // Retrieve the last joined client in the room (could be used for validation).
            var lastClient = await _clientService.GetLastJoinedClientAsync(roomCode);

            _logger.LogInformation("Client {ConnectionId} sent offer for {TargetConnectionId}", connectionId, targetConnectionId);
            await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", connectionId, offer);
        }

        /// <summary>
        /// Sends an answer from the caller to a specified target client.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <param name="targetConnectionId">The target client's connection ID.</param>
        /// <param name="answer">The WebRTC answer.</param>
        public async Task SendAnswer(string roomCode, string targetConnectionId, string answer)
        {
            _logger.LogInformation("Client {ConnectionId} sent answer for {TargetConnectionId}", Context.ConnectionId, targetConnectionId);
            await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        /// <summary>
        /// Sends an ICE candidate from the caller to a specified target client.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <param name="targetConnectionId">The target client's connection ID.</param>
        /// <param name="candidate">The ICE candidate in JSON format.</param>
        public async Task SendIceCandidate(string roomCode, string targetConnectionId, string candidate)
        {
            _logger.LogInformation("Client {ConnectionId} sent ICE candidate for {TargetConnectionId}", Context.ConnectionId, targetConnectionId);
            await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        /// <summary>
        /// Returns the current connection ID.
        /// </summary>
        /// <returns>The connection ID of the current client.</returns>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}

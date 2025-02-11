

namespace GMB.Domain.Entities
{
    /// <summary>
    /// Represents a client (or player) in the system.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Gets or sets the unique identifier for the client.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// Defaults to "Guest" if not specified.
        /// </summary>
        public string Name { get; set; } = "Guest";

        /// <summary>
        /// Gets or sets the SignalR connection identifier associated with the client.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the public IP address of the client.
        /// This is used for NAT traversal and establishing P2P connections.
        /// </summary>
        public string? PublicIp { get; set; }

        /// <summary>
        /// Gets or sets the public port of the client.
        /// This is used for NAT traversal and establishing P2P connections.
        /// </summary>
        public int? PublicPort { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the room the client has joined.
        /// </summary>
        public Guid? RoomId { get; set; }

        /// <summary>
        /// Gets or sets the room entity that the client is part of.
        /// </summary>
        public Room? Room { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the client joined the room.
        /// This field is useful for tracking connection time.
        /// </summary>
        public DateTime? JoinedAt { get; set; }
    }
}



namespace GMB.Domain.Entities
{
    /// <summary>
    /// Represents a room that can host multiple clients.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Gets or sets the unique identifier for the room.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the room code used to uniquely identify the room.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of players allowed in the room.
        /// </summary>
        public int MaxPlayers { get; set; } = 10;

        /// <summary>
        /// Gets or sets the list of clients currently in the room.
        /// </summary>
        public List<Client> Clients { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the room is currently private.
        /// </summary> 
        public bool IsPrivate { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the room is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the row version for concurrency control.
        /// </summary>
        public byte[] RowVersion { get; set; }
    }
}

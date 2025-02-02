namespace GMB.Domain.Entities
{
    public class Room
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 10;
        public List<Client> Clients { get; set; } = new();
        public bool IsActive { get; set; }

        public byte[] RowVersion { get; set; }

    }
}

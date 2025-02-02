namespace GMB.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Guest"; 
        public string ConnectionId { get; set; }
        public string? PublicIp { get; set; } 
        public int? PublicPort { get; set; }


        public Guid? RoomId { get; set; } 
        public Room? Room { get; set; }
    }
}


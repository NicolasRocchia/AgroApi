namespace APIAgroConnect.Domain.Entities
{
    public class Municipality : BaseAuditableEntity
    {
        public string Name { get; set; } = null!;
        public string? Province { get; set; }
        public string? Department { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public long? UserId { get; set; }
        public User? User { get; set; }
    }
}

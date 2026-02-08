namespace APIAgroConnect.Domain.Entities
{
    public class Advisor : BaseAuditableEntity
    {
        public string FullName { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public string? Contact { get; set; }
    }
}

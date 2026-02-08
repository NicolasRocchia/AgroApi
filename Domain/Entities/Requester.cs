namespace APIAgroConnect.Domain.Entities
{
    public class Requester : BaseAuditableEntity
    {
        public string LegalName { get; set; } = null!;
        public string TaxId { get; set; } = null!;
        public string? Address { get; set; }
        public string? Contact { get; set; }
    }
}

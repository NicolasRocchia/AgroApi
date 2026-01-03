namespace APIAgroConnect.Domain.Entities
{
    public abstract class BaseAuditableEntity : BaseEntity
    {      
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long? CreatedByUserId { get; set; }
        public long? UpdatedByUserId { get; set; }
        public long? DeletedByUserId { get; set; }
    }
}

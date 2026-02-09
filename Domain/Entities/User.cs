namespace APIAgroConnect.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; } = null!;
        public string EmailNormalized { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? TaxId { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long? CreatedByUserId { get; set; }
        public long? UpdatedByUserId { get; set; }
        public long? DeletedByUserId { get; set; }

        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
    }
}
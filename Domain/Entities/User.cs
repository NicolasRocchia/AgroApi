namespace APIAgroConnect.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }

        public string UserName { get; set; } = null!;
        public string EmailNormalized { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public bool IsBlocked { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

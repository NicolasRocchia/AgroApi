namespace APIAgroConnect.Domain.Entities
{
    public class User : BaseAuditableEntity
    {
        public string UserName { get; set; } = null!;
        public string EmailNormalized { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public bool IsBlocked { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

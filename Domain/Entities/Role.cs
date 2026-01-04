namespace APIAgroConnect.Domain.Entities
{    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public short AccessLevel { get; set; }
        public string? Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

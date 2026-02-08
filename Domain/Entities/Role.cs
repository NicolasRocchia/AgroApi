using APIAgroConnect.Domain.Entities;

public class Role
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public short AccessLevel { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public long? CreatedByUserId { get; set; }
    public long? UpdatedByUserId { get; set; }
    public long? DeletedByUserId { get; set; }

    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}

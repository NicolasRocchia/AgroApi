using Microsoft.AspNetCore.Identity;

namespace APIAgroConnect.Domain.Entities
{
    public class UserRoles : IdentityUserRole<long>
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
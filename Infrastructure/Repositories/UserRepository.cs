using APIAgroConnect.Domain.Entities;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Infrastructure.Repositories
{
    public class UserRepository
    {
        private readonly AgroDbContext _context;

        public UserRepository(AgroDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string emailNormalized)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.EmailNormalized == emailNormalized &&
                    u.DeletedAt == null);
        }

        public Task SaveChangesAsync()
    => _context.SaveChangesAsync();
    }
}

using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Domain.Entities;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AgroDbContext _db;

        public AdminUserService(AgroDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserListItemDto>> GetAllUsersAsync()
        {
            return await _db.Users
                .IgnoreQueryFilters()
                .Where(u => u.DeletedAt == null)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.EmailNormalized,
                    TaxId = u.TaxId,
                    PhoneNumber = u.PhoneNumber,
                    IsBlocked = u.IsBlocked,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<RegisterResponse> CreateUserAsync(AdminCreateUserRequest request, long actorUserId)
        {
            var emailNormalized = request.Email.Trim().ToUpperInvariant();
            var taxId = request.TaxId.Trim();

            // Validar email único
            var emailExists = await _db.Users.AnyAsync(u => u.EmailNormalized == emailNormalized);
            if (emailExists)
                throw new InvalidOperationException("Ya existe una cuenta con ese email.");

            // Validar CUIT/CUIL único
            var taxIdExists = await _db.Users.AnyAsync(u => u.TaxId == taxId);
            if (taxIdExists)
                throw new InvalidOperationException("Ya existe una cuenta con ese CUIT/CUIL.");

            // Verificar que el rol existe
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
            if (role == null)
                throw new InvalidOperationException("El rol especificado no existe.");

            var now = DateTime.UtcNow;

            var user = new User
            {
                UserName = request.UserName.Trim(),
                EmailNormalized = emailNormalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                TaxId = taxId,
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsBlocked = false,
                CreatedAt = now,
                CreatedByUserId = actorUserId
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.UserRoles.Add(new UserRoles
            {
                UserId = user.Id,
                RoleId = request.RoleId,
                CreatedAt = now
            });

            await _db.SaveChangesAsync();

            return new RegisterResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.EmailNormalized,
                Role = role.Name,
                Message = $"Usuario creado exitosamente con rol {role.Name}."
            };
        }

        public async Task ToggleBlockAsync(long userId, bool isBlocked, long actorUserId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            user.IsBlocked = isBlocked;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = actorUserId;

            await _db.SaveChangesAsync();
        }

        public async Task ChangeRoleAsync(long userId, long newRoleId, long actorUserId)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == newRoleId);
            if (role == null)
                throw new InvalidOperationException("El rol especificado no existe.");

            // Remover roles actuales
            _db.UserRoles.RemoveRange(user.UserRoles);

            // Asignar nuevo rol
            _db.UserRoles.Add(new UserRoles
            {
                UserId = user.Id,
                RoleId = newRoleId,
                CreatedAt = DateTime.UtcNow
            });

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = actorUserId;

            await _db.SaveChangesAsync();
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return await _db.Roles
                .OrderByDescending(r => r.AccessLevel)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    AccessLevel = r.AccessLevel,
                    Description = r.Description
                })
                .ToListAsync();
        }
    }
}

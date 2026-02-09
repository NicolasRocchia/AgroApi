using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Common.Security;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Domain.Entities;
using APIAgroConnect.Infrastructure.Data;
using APIAgroConnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly AgroDbContext _db;
        private readonly JwtTokenService _jwt;

        private const long RoleAplicadorId = 3; // Aplicador = rol por defecto en registro público

        public AuthService(UserRepository userRepository, AgroDbContext db, JwtTokenService jwt)
        {
            _userRepository = userRepository;
            _db = db;
            _jwt = jwt;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var emailNormalized = NormalizeEmail(request.Email);

            var user = await _userRepository.GetByEmailAsync(emailNormalized);

            if (user == null || user.IsBlocked)
            {
                return null;
            }

            var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!ok)
            {
                return null;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();

            var roles = user.UserRoles.Select(r => r.Role.Name).ToList();
            var (token, expiresAt) = _jwt.CreateToken(user, roles);

            return new LoginResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.EmailNormalized,
                Roles = roles,
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var emailNormalized = NormalizeEmail(request.Email);
            var taxId = request.TaxId.Trim();

            // Validar email único
            var emailExists = await _db.Users
                .AnyAsync(u => u.EmailNormalized == emailNormalized);

            if (emailExists)
                throw new InvalidOperationException("Ya existe una cuenta con ese email.");

            // Validar CUIT/CUIL único
            var taxIdExists = await _db.Users
                .AnyAsync(u => u.TaxId == taxId);

            if (taxIdExists)
                throw new InvalidOperationException("Ya existe una cuenta con ese CUIT/CUIL.");

            // Verificar que el rol Aplicador existe
            var roleExists = await _db.Roles.AnyAsync(r => r.Id == RoleAplicadorId);
            if (!roleExists)
                throw new InvalidOperationException("Error de configuración: rol Aplicador no encontrado.");

            var now = DateTime.UtcNow;

            var user = new User
            {
                UserName = request.UserName.Trim(),
                EmailNormalized = emailNormalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                TaxId = taxId,
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsBlocked = false,
                CreatedAt = now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Asignar rol Aplicador
            _db.UserRoles.Add(new UserRoles
            {
                UserId = user.Id,
                RoleId = RoleAplicadorId,
                CreatedAt = now
            });

            await _db.SaveChangesAsync();

            return new RegisterResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.EmailNormalized,
                Role = "Aplicador",
                Message = "Cuenta creada exitosamente."
            };
        }

        private static string NormalizeEmail(string email)
            => (email ?? "").Trim().ToUpperInvariant();
    }
}

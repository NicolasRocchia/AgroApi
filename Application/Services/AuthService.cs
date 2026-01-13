using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Common.Security;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Infrastructure.Repositories;

namespace APIAgroConnect.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly JwtTokenService _jwt;

        public AuthService(UserRepository userRepository, JwtTokenService jwt)
        {
            _userRepository = userRepository;
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

        private static string NormalizeEmail(string email)
            => (email ?? "").Trim().ToUpperInvariant();
    }
}

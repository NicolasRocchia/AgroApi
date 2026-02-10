using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IAdminUserService
    {
        Task<List<UserListItemDto>> GetAllUsersAsync();
        Task<RegisterResponse> CreateUserAsync(AdminCreateUserRequest request, long actorUserId);
        Task ToggleBlockAsync(long userId, bool isBlocked, long actorUserId);
        Task ChangeRoleAsync(long userId, long newRoleId, long actorUserId);
        Task<List<RoleDto>> GetRolesAsync();
    }
}

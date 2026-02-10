using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Application.Services;
using APIAgroConnect.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIAgroConnect.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminUserService _userService;
        private readonly IInsightsService _insightsService;

        public AdminController(IAdminUserService userService, IInsightsService insightsService)
        {
            _userService = userService;
            _insightsService = insightsService;
        }

        /// <summary>
        /// Estadísticas y métricas del sistema
        /// </summary>
        [HttpGet("insights")]
        public async Task<IActionResult> GetInsights()
        {
            try
            {
                var insights = await _insightsService.GetInsightsAsync();
                return Ok(insights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener estadísticas.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos los roles disponibles
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _userService.GetRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Lista todos los usuarios del sistema
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Crea un nuevo usuario con un rol específico
        /// </summary>
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { errors });
            }

            try
            {
                var actorUserId = GetUserIdOrThrow();
                var result = await _userService.CreateUserAsync(request, actorUserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno al crear el usuario.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Bloquea o desbloquea un usuario
        /// </summary>
        [HttpPut("users/{id}/block")]
        public async Task<IActionResult> ToggleBlock(long id, [FromBody] AdminToggleBlockRequest request)
        {
            try
            {
                var actorUserId = GetUserIdOrThrow();
                
                if (id == actorUserId)
                    return BadRequest(new { error = "No podés bloquearte a vos mismo." });

                await _userService.ToggleBlockAsync(id, request.IsBlocked, actorUserId);
                return Ok(new { message = request.IsBlocked ? "Usuario bloqueado." : "Usuario desbloqueado." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el rol de un usuario
        /// </summary>
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeRole(long id, [FromBody] AdminChangeRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var actorUserId = GetUserIdOrThrow();
                await _userService.ChangeRoleAsync(id, request.RoleId, actorUserId);
                return Ok(new { message = "Rol actualizado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno.", detail = ex.Message });
            }
        }

        private long GetUserIdOrThrow()
        {
            var sub = User.FindFirstValue("sub");
            if (!string.IsNullOrWhiteSpace(sub) && long.TryParse(sub, out var idFromSub))
                return idFromSub;

            var nid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(nid) && long.TryParse(nid, out var idFromNid))
                return idFromNid;

            throw new UnauthorizedAccessException("Token inválido.");
        }
    }
}

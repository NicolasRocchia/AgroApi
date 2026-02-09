using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = APIAgroConnect.Contracts.Requests.LoginRequest;
using RegisterRequest = APIAgroConnect.Contracts.Requests.RegisterRequest;

namespace APIAgroConnect.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized("Credenciales inválidas");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
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
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno al crear la cuenta.", detail = ex.Message });
            }
        }
    }
}
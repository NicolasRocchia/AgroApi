using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace APIAgroConnect.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MunicipalitiesController : ControllerBase
    {
        private readonly IMunicipalityService _municipalityService;

        public MunicipalitiesController(IMunicipalityService municipalityService)
        {
            _municipalityService = municipalityService;
        }

        private long GetUserIdOrThrow()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Token inválido.");
            return long.Parse(sub);
        }

        // ─── CRUD ──────────────────────────────

        /// <summary>Lista todos los municipios</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _municipalityService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>Lista municipios ordenados por cercanía a un punto GPS</summary>
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearby(
            [FromQuery] string lat,
            [FromQuery] string lng,
            [FromQuery] int limit = 10)
        {
            // Limpiar y normalizar: Google Maps copia "-31.444928, -62.140988"
            // En cultura es-AR el decimal es coma, así que normalizamos todo a punto
            var latClean = (lat ?? "").Trim().Replace(",", ".");
            var lngClean = (lng ?? "").Trim().Replace(",", ".");

            if (!decimal.TryParse(latClean, NumberStyles.Any, CultureInfo.InvariantCulture, out var latDecimal) ||
                !decimal.TryParse(lngClean, NumberStyles.Any, CultureInfo.InvariantCulture, out var lngDecimal))
            {
                return BadRequest(new { error = "Coordenadas inválidas. Usá formato decimal con punto, ej: -31.4449" });
            }

            var result = await _municipalityService.GetNearbyAsync(latDecimal, lngDecimal, limit);
            return Ok(result);
        }

        /// <summary>Obtiene un municipio por ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _municipalityService.GetByIdAsync(id);
            if (result == null) return NotFound(new { error = "Municipio no encontrado." });
            return Ok(result);
        }

        /// <summary>Crea un nuevo municipio (solo Admin)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMunicipalityRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserIdOrThrow();
            var result = await _municipalityService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Actualiza un municipio (solo Admin)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateMunicipalityRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserIdOrThrow();
            await _municipalityService.UpdateAsync(id, request, userId);
            return Ok(new { message = "Municipio actualizado." });
        }

        // ─── FLUJO APLICADOR ──────────────────

        /// <summary>Aplicador envía receta a un municipio</summary>
        [HttpPost("/api/recipes/{recipeId}/assign-municipality")]
        [Authorize(Roles = "Aplicador")]
        public async Task<IActionResult> AssignMunicipality(long recipeId, [FromBody] AssignMunicipalityRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserIdOrThrow();
            await _municipalityService.AssignToMunicipalityAsync(recipeId, request.MunicipalityId, userId);
            return Ok(new { message = "Receta enviada al municipio correctamente." });
        }

        // ─── FLUJO MUNICIPIO ──────────────────

        /// <summary>Municipio revisa una receta (aprobar/rechazar/observar/redirigir)</summary>
        [HttpPost("/api/recipes/{recipeId}/review")]
        [Authorize(Roles = "Municipio")]
        public async Task<IActionResult> ReviewRecipe(long recipeId, [FromBody] ReviewRecipeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserIdOrThrow();
            var municipalityId = await _municipalityService.GetMunicipalityIdByUserIdAsync(userId)
                ?? throw new InvalidOperationException("Tu usuario no está asociado a ningún municipio.");

            await _municipalityService.ReviewRecipeAsync(recipeId, request, municipalityId, userId);
            return Ok(new { message = $"Receta {request.Action.ToUpper()} correctamente." });
        }

        // ─── MENSAJES ────────────────────────

        /// <summary>Envía un mensaje en una receta</summary>
        [HttpPost("/api/recipes/{recipeId}/messages")]
        [Authorize(Roles = "Aplicador,Municipio")]
        public async Task<IActionResult> SendMessage(long recipeId, [FromBody] SendRecipeMessageRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserIdOrThrow();
            var result = await _municipalityService.SendMessageAsync(recipeId, request.Message, userId);
            return Ok(result);
        }

        /// <summary>Obtiene los mensajes de una receta</summary>
        [HttpGet("/api/recipes/{recipeId}/messages")]
        [Authorize(Roles = "Aplicador,Municipio,Admin")]
        public async Task<IActionResult> GetMessages(long recipeId)
        {
            var result = await _municipalityService.GetMessagesAsync(recipeId);
            return Ok(result);
        }
    }
}
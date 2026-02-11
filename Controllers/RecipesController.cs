using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIAgroConnect.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeImportService _importService;
        private readonly IRecipeService _recipeService;

        public RecipesController(
            IRecipeImportService importService,
            IRecipeService recipeService)
        {
            _importService = importService;
            _recipeService = recipeService;
        }

        /// <summary>
        /// Obtiene un listado paginado de recetas con filtros opcionales
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRecipes([FromQuery] RecipeQueryRequest request)
        {
            if (User.IsInRole("Aplicador"))
            {
                request.CreatedByUserId = GetUserIdOrThrow();
            }
            else if (User.IsInRole("Municipio"))
            {
                var userId = GetUserIdOrThrow();
                var municipality = await _recipeService.GetMunicipalityByUserIdAsync(userId);
                
                if (municipality == null)
                    return BadRequest(new { error = "Usuario no tiene municipio asignado." });
                
                request.MunicipalityId = municipality.Id;
            }

            var result = await _recipeService.GetRecipesAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el detalle completo de una receta por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetRecipeById(long id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);

            if (recipe == null)
                return NotFound(new { error = $"No se encontró la receta con ID {id}" });

            if (User.IsInRole("Aplicador"))
            {
                var userId = GetUserIdOrThrow();
                if (recipe.CreatedByUserId != userId)
                    return Forbid();
            }
            else if (User.IsInRole("Municipio"))
            {
                var userId = GetUserIdOrThrow();
                var municipality = await _recipeService.GetMunicipalityByUserIdAsync(userId);
                
                if (municipality == null)
                    return BadRequest(new { error = "Usuario no tiene municipio asignado." });
                
                if (recipe.AssignedMunicipalityId != municipality.Id)
                    return Forbid();
            }

            return Ok(recipe);
        }

        [HttpPost("import-pdf")]
        [Authorize]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPdf([FromForm] ImportRecipePdfRequest request)
        {
            var userId = GetUserIdOrThrow();
            var result = await _importService.ImportAsync(request.Pdf, userId, request.DryRun);
            return Ok(result);
        }

        /// <summary>
        /// Cambia el estado de una receta (ABIERTA → CERRADA o ANULADA)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> ChangeStatus(long id, [FromBody] ChangeRecipeStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdOrThrow();

            // Aplicador solo puede cambiar sus propias recetas
            if (User.IsInRole("Aplicador"))
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe == null)
                    return NotFound(new { error = $"No se encontró la receta con ID {id}" });
                if (recipe.CreatedByUserId != userId)
                    return Forbid();
            }

            await _recipeService.ChangeStatusAsync(id, request.Status, userId);
            return Ok(new { message = $"Estado actualizado a {request.Status.ToUpper()}." });
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

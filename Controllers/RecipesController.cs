using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Infrastructure.Data;
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
            try
            {
                // Si el usuario es Aplicador, solo ve sus propias recetas
                if (User.IsInRole("Aplicador"))
                {
                    var userId = GetUserIdOrThrow();
                    request.CreatedByUserId = userId;
                }

                var result = await _recipeService.GetRecipesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.ToString() });
            }
        }

        /// <summary>
        /// Obtiene el detalle completo de una receta por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetRecipeById(long id)
        {
            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);

                if (recipe == null)
                    return NotFound(new { message = $"No se encontró la receta con ID {id}" });

                // Aplicador solo puede ver sus propias recetas
                if (User.IsInRole("Aplicador"))
                {
                    var userId = GetUserIdOrThrow();
                    if (recipe.CreatedByUserId != userId)
                        return Forbid();
                }

                return Ok(recipe);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.ToString() });
            }
        }

        [HttpPost("import-pdf")]
        [Authorize]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPdf([FromForm] ImportRecipePdfRequest request)
        {
            try
            {
                var userId = GetUserIdOrThrow();

                var result = await _importService.ImportAsync(request.Pdf, userId, request.DryRun);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.ToString() });
            }
        }

        private long GetUserIdOrThrow()
        {
            // 1) Intentar sub
            var sub = User.FindFirstValue("sub");
            if (!string.IsNullOrWhiteSpace(sub) && long.TryParse(sub, out var idFromSub))
                return idFromSub;

            // 2) Intentar NameIdentifier
            var nid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(nid) && long.TryParse(nid, out var idFromNid))
                return idFromNid;

            throw new UnauthorizedAccessException("Token inválido: no trae claim sub ni NameIdentifier.");
        }
    }
}

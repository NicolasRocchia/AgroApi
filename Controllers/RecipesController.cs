using APIAgroConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIAgroConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/recipes")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeImportService _importService;

        public RecipesController(IRecipeImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("import-pdf")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> ImportPdf([FromForm] IFormFile pdf, [FromQuery] bool dryRun = true)
        {
            if (pdf is null || pdf.Length == 0)
                return BadRequest("PDF requerido.");

            if (!pdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("El archivo debe ser .pdf");

            var actorUserId = GetUserId();
            var result = await _importService.ImportAsync(pdf, actorUserId, dryRun);

            return Ok(result);
        }

        private long GetUserId()
        {
            var v = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(v, out var id) ? id : 0;
        }
    }
}

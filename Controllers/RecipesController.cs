using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIAgroConnect.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeImportService _importService;
        private readonly AgroDbContext _db;

        public RecipesController(IRecipeImportService importService, AgroDbContext db)
        {
            _importService = importService;
            _db = db;
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

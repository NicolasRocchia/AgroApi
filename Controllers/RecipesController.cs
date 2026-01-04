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

        public RecipesController(IRecipeImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("upload-test")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadTest([FromForm] ImportRecipePdfRequest request)
        {
            // Solo lee bytes, no parsea PDF
            await using var ms = new MemoryStream();
            await request.Pdf.CopyToAsync(ms);

            return Ok(new
            {
                fileName = request.Pdf.FileName,
                length = request.Pdf.Length,
                bytesRead = ms.Length
            });
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

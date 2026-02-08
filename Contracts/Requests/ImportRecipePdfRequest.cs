using Microsoft.AspNetCore.Mvc;

namespace APIAgroConnect.Contracts.Requests
{
    public class ImportRecipePdfRequest
    {
        [FromForm(Name = "Pdf")]
        public IFormFile Pdf { get; set; } = null!;
        [FromForm(Name = "DryRun")]
        public bool DryRun { get; set; } = false;
    }
}

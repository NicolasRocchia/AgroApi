using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Models;
using APIAgroConnect.Domain.Entities;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public sealed class RecipeImportService : IRecipeImportService
    {
        private readonly IPdfTextExtractor _extractor;
        private readonly IRecipePdfParser _parser;
        private readonly AgroDbContext _db;

        public RecipeImportService(
            IPdfTextExtractor extractor,
            IRecipePdfParser parser,
            AgroDbContext db)
        {
            _extractor = extractor;
            _parser = parser;
            _db = db;
        }

        public async Task<object> ImportAsync(IFormFile pdf, long actorUserId, bool dryRun)
        {
            if (pdf is null || pdf.Length == 0)
                throw new ArgumentException("PDF requerido.", nameof(pdf));

            // Leer el archivo una sola vez
            byte[] bytes;
            await using (var ms = new MemoryStream())
            {
                await pdf.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            // 1) Extraer texto (stream nuevo)
            string rawText;
            await using (var textStream = new MemoryStream(bytes, writable: false))
            {
                rawText = await _extractor.ExtractTextAsync(textStream);
            }

            // 2) Parsear completo (texto + stream para tabla)
            ParsedRecipe parsed;
            await using (var parseStream = new MemoryStream(bytes, writable: false))
            {
                parsed = _parser.Parse(parseStream, rawText);
            }

            if (dryRun)
                return new { parsed };

            var now = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync();

            var requester = await UpsertRequesterAsync(parsed, actorUserId, now);
            var advisor = await UpsertAdvisorAsync(parsed, actorUserId, now);

            var recipe = await _db.Recipes
                .Include(r => r.Products)
                .Include(r => r.Lots).ThenInclude(l => l.Vertices)
                .Include(r => r.SensitivePoints)
                .FirstOrDefaultAsync(r => r.RfdNumber == parsed.RfdNumber);

            if (recipe is null)
            {
                recipe = new Recipe
                {
                    RfdNumber = parsed.RfdNumber,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId,
                    Products = new List<RecipeProduct>(),
                    Lots = new List<RecipeLot>(),
                    SensitivePoints = new List<RecipeSensitivePoint>()
                };
                _db.Recipes.Add(recipe);
            }
            else
            {
                recipe.UpdatedAt = now;
                recipe.UpdatedByUserId = actorUserId;
            }

            // Asegurar IDs si Requester/Advisor son nuevos
            await _db.SaveChangesAsync();

            MapRecipeHeader(recipe, parsed, requester, advisor);

            SoftDeleteChildren(recipe, now, actorUserId);
            AddChildrenFromParsed(recipe, parsed, now, actorUserId);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new
            {
                ok = true,
                recipeId = recipe.Id,
                rfdNumber = parsed.RfdNumber
            };
        }

        private async Task<Requester> UpsertRequesterAsync(ParsedRecipe parsed, long actorUserId, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(parsed.RequesterTaxId))
                throw new InvalidOperationException("El PDF no trae CUIT/CUIL del solicitante.");

            var requester = await _db.Requesters
                .FirstOrDefaultAsync(x => x.TaxId == parsed.RequesterTaxId);

            if (requester is null)
            {
                requester = new Requester
                {
                    LegalName = parsed.RequesterName ?? "",
                    TaxId = parsed.RequesterTaxId,
                    Address = null,
                    Contact = null,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };
                _db.Requesters.Add(requester);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(parsed.RequesterName))
                    requester.LegalName = parsed.RequesterName;

                requester.UpdatedAt = now;
                requester.UpdatedByUserId = actorUserId;
            }

            return requester;
        }

        private async Task<Advisor> UpsertAdvisorAsync(ParsedRecipe parsed, long actorUserId, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(parsed.AdvisorLicense))
                throw new InvalidOperationException("El PDF no trae matrícula (M.P.) del asesor.");

            var advisor = await _db.Advisors
                .FirstOrDefaultAsync(x => x.LicenseNumber == parsed.AdvisorLicense);

            if (advisor is null)
            {
                advisor = new Advisor
                {
                    FullName = parsed.AdvisorName ?? "",
                    LicenseNumber = parsed.AdvisorLicense,
                    Contact = null,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };
                _db.Advisors.Add(advisor);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(parsed.AdvisorName))
                    advisor.FullName = parsed.AdvisorName;

                advisor.UpdatedAt = now;
                advisor.UpdatedByUserId = actorUserId;
            }

            return advisor;
        }

        private static void MapRecipeHeader(Recipe recipe, ParsedRecipe parsed, Requester requester, Advisor advisor)
        {
            recipe.Status = parsed.Status;

            recipe.IssueDate = parsed.IssueDate.ToDateTime(TimeOnly.MinValue);
            recipe.PossibleStartDate = parsed.PossibleStartDate?.ToDateTime(TimeOnly.MinValue);
            recipe.RecommendedDate = parsed.RecommendedDate?.ToDateTime(TimeOnly.MinValue);
            recipe.ExpirationDate = parsed.ExpirationDate?.ToDateTime(TimeOnly.MinValue);

            recipe.RequesterId = requester.Id;
            recipe.AdvisorId = advisor.Id;

            recipe.ApplicationType = parsed.ApplicationType;
            recipe.Crop = parsed.Crop;
            recipe.Diagnosis = parsed.Diagnosis;
            recipe.Treatment = parsed.Treatment;
            recipe.MachineToUse = parsed.MachineToUse;

            recipe.UnitSurfaceHa = parsed.UnitSurfaceHa;

            recipe.TempMin = parsed.TempMin;
            recipe.TempMax = parsed.TempMax;
            recipe.HumidityMin = parsed.HumidityMin;
            recipe.HumidityMax = parsed.HumidityMax;
            recipe.WindMinKmh = parsed.WindMinKmh;
            recipe.WindMaxKmh = parsed.WindMaxKmh;
            recipe.WindDirection = parsed.WindDirection;

            recipe.Notes = parsed.Notes;
        }

        private static void SoftDeleteChildren(Recipe recipe, DateTime now, long actorUserId)
        {
            foreach (var p in recipe.Products)
            {
                p.DeletedAt = now;
                p.DeletedByUserId = actorUserId;
            }

            foreach (var lot in recipe.Lots)
            {
                foreach (var v in lot.Vertices)
                    v.DeletedAt = now;

                lot.DeletedAt = now;
                lot.DeletedByUserId = actorUserId;
            }

            foreach (var sp in recipe.SensitivePoints)
            {
                sp.DeletedAt = now;
                sp.DeletedByUserId = actorUserId;
            }
        }

        private static void AddChildrenFromParsed(Recipe recipe, ParsedRecipe parsed, DateTime now, long actorUserId)
        {
            foreach (var p in parsed.Products)
            {
                recipe.Products.Add(new RecipeProduct
                {
                    ProductType = p.ProductType,
                    ProductName = p.ProductName,
                    SenasaRegistry = p.SenasaRegistry,
                    ToxicologicalClass = p.ToxicologicalClass,
                    DoseValue = p.DoseValue,
                    DoseUnit = p.DoseUnit,
                    DosePerUnit = p.DosePerUnit,
                    TotalValue = p.TotalValue,
                    TotalUnit = p.TotalUnit,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                });
            }

            foreach (var l in parsed.Lots)
            {
                var lot = new RecipeLot
                {
                    LotName = l.LotName,
                    Locality = l.Locality,
                    Department = l.Department,
                    SurfaceHa = l.SurfaceHa,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId,
                    Vertices = new List<RecipeLotVertex>()
                };

                foreach (var v in l.Vertices.OrderBy(x => x.Order))
                {
                    lot.Vertices.Add(new RecipeLotVertex
                    {
                        Order = v.Order,
                        Latitude = v.Latitude,
                        Longitude = v.Longitude,
                        CreatedAt = now
                    });
                }

                recipe.Lots.Add(lot);
            }
        }
    }
}

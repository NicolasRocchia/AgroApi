using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Models;
using APIAgroConnect.Domain.Entities;            // tus entities
using APIAgroConnect.Infrastructure.Data;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public sealed class RecipeImportService : IRecipeImportService
    {
        private readonly IPdfTextExtractor _extractor;
        private readonly IRecipePdfParser _parser;
        private readonly AgroDbContext _db;

        public RecipeImportService(IPdfTextExtractor extractor, IRecipePdfParser parser, AgroDbContext db)
        {
            _extractor = extractor;
            _parser = parser;
            _db = db;
        }

        public async Task<object> ImportAsync(IFormFile pdf, long actorUserId, bool dryRun)
        {
            await using var ms = new MemoryStream();
            await pdf.CopyToAsync(ms);
            ms.Position = 0;

            var text = await _extractor.ExtractTextAsync(ms);
            var parsed = _parser.Parse(text);

            if (dryRun)
                return new { parsed };

            var now = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync();

            // 1) Upsert Requester (por TaxId)
            var requester = await _db.Requesters
                .FirstOrDefaultAsync(x => x.TaxId == parsed.RequesterTaxId && x.DeletedAt == null);

            if (requester == null)
            {
                requester = new Requester
                {
                    LegalName = parsed.RequesterName,
                    TaxId = parsed.RequesterTaxId,
                    Address = null,
                    Contact = null,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };
                _db.Requesters.Add(requester);
                await _db.SaveChangesAsync();
            }
            else
            {
                requester.LegalName = parsed.RequesterName;
                requester.UpdatedAt = now;
                requester.UpdatedByUserId = actorUserId;
                await _db.SaveChangesAsync();
            }

            // 2) Upsert Advisor (por LicenseNumber)
            var advisor = await _db.Advisors
                .FirstOrDefaultAsync(x => x.LicenseNumber == parsed.AdvisorLicense && x.DeletedAt == null);

            if (advisor == null)
            {
                advisor = new Advisor
                {
                    FullName = parsed.AdvisorName,
                    LicenseNumber = parsed.AdvisorLicense,
                    Contact = null,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };
                _db.Advisors.Add(advisor);
                await _db.SaveChangesAsync();
            }
            else
            {
                advisor.FullName = parsed.AdvisorName;
                advisor.UpdatedAt = now;
                advisor.UpdatedByUserId = actorUserId;
                await _db.SaveChangesAsync();
            }

            // 3) Upsert Recipe (por RfdNumber)
            var recipe = await _db.Recipes
                .Include(r => r.Products.Where(p => p.DeletedAt == null))
                .Include(r => r.Lots.Where(l => l.DeletedAt == null))
                    .ThenInclude(l => l.Vertices.Where(v => v.DeletedAt == null))
                .Include(r => r.SensitivePoints.Where(s => s.DeletedAt == null))
                .FirstOrDefaultAsync(r => r.RfdNumber == parsed.RfdNumber && r.DeletedAt == null);

            if (recipe == null)
            {
                recipe = new Recipe
                {
                    RfdNumber = parsed.RfdNumber,
                    Status = parsed.Status,

                    IssueDate = parsed.IssueDate.ToDateTime(TimeOnly.MinValue),
                    PossibleStartDate = parsed.PossibleStartDate?.ToDateTime(TimeOnly.MinValue),
                    RecommendedDate = parsed.RecommendedDate?.ToDateTime(TimeOnly.MinValue),
                    ExpirationDate = parsed.ExpirationDate?.ToDateTime(TimeOnly.MinValue),

                    RequesterId = requester.Id,
                    AdvisorId = advisor.Id,

                    ApplicationType = parsed.ApplicationType,
                    Crop = parsed.Crop,
                    Diagnosis = parsed.Diagnosis,
                    Treatment = parsed.Treatment,
                    MachineToUse = parsed.MachineToUse,

                    UnitSurfaceHa = parsed.UnitSurfaceHa,

                    TempMin = parsed.TempMin,
                    TempMax = parsed.TempMax,
                    HumidityMin = parsed.HumidityMin,
                    HumidityMax = parsed.HumidityMax,
                    WindMinKmh = parsed.WindMinKmh,
                    WindMaxKmh = parsed.WindMaxKmh,
                    WindDirection = parsed.WindDirection,

                    Notes = parsed.Notes,

                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };

                _db.Recipes.Add(recipe);
                await _db.SaveChangesAsync();
            }
            else
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

                recipe.UpdatedAt = now;
                recipe.UpdatedByUserId = actorUserId;

                await _db.SaveChangesAsync();
            }

            // 4) Sync children (soft-delete + insert)
            SoftDeleteChildren(recipe, now, actorUserId);
            await _db.SaveChangesAsync();

            InsertChildrenFromParsed(recipe.Id, parsed, now, actorUserId);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return new { ok = true, recipeId = recipe.Id, parsed.RfdNumber };
        }

        private void SoftDeleteChildren(Recipe recipe, DateTime now, long actorUserId)
        {
            foreach (var p in recipe.Products)
            {
                p.DeletedAt = now;
                p.DeletedByUserId = actorUserId;
            }

            foreach (var lot in recipe.Lots)
            {
                // borrar vertices primero
                foreach (var v in lot.Vertices)
                {
                    v.DeletedAt = now;
                }

                lot.DeletedAt = now;
                lot.DeletedByUserId = actorUserId;
            }

            foreach (var sp in recipe.SensitivePoints)
            {
                sp.DeletedAt = now;
                sp.DeletedByUserId = actorUserId;
            }
        }

        private void InsertChildrenFromParsed(long recipeId, ParsedRecipe parsed, DateTime now, long actorUserId)
        {
            // Products
            foreach (var p in parsed.Products)
            {
                _db.RecipeProducts.Add(new RecipeProduct
                {
                    RecipeId = recipeId,
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

            // Lots + vertices
            foreach (var l in parsed.Lots)
            {
                var lot = new RecipeLot
                {
                    RecipeId = recipeId,
                    LotName = l.LotName,
                    Locality = l.Locality,
                    Department = l.Department,
                    SurfaceHa = l.SurfaceHa,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };

                _db.RecipeLots.Add(lot);
                _db.SaveChanges(); // para obtener lot.Id (si no querés SaveChanges acá, hacelo luego con temp ids)

                foreach (var v in l.Vertices)
                {
                    _db.RecipeLotVertices.Add(new RecipeLotVertex
                    {
                        LotId = lot.Id,
                        Order = v.Order,
                        Latitude = v.Latitude,
                        Longitude = v.Longitude,
                        CreatedAt = now
                    });
                }
            }

            // Sensitive points (si tu parser los llena)
            // foreach (var s in parsed.SensitivePoints) { ... }
        }
    }
}

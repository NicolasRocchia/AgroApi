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

            // 2) Parsear completo (texto + stream para tablas)
            ParsedRecipe parsed;
            await using (var parseStream = new MemoryStream(bytes, writable: false))
            {
                parsed = _parser.Parse(parseStream, rawText);
            }

            if (dryRun)
                return new { parsed };

            var now = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync();

            // Upsert de Requester/Advisor (incluye revivir soft-deleted)
            var requester = await UpsertRequesterAsync(parsed, actorUserId, now);
            var advisor = await UpsertAdvisorAsync(parsed, actorUserId, now);

            // Guardar para asegurar IDs si eran nuevos
            await _db.SaveChangesAsync();

            // Buscar receta por RfdNumber INCLUYENDO soft-deleted, para poder revivirla
            var recipe = await _db.Recipes
                .IgnoreQueryFilters()
                .Include(r => r.Products) // RecipeProducts
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
                // Revivir si estaba soft-deleted
                if (recipe.DeletedAt != null)
                {
                    recipe.DeletedAt = null;
                    recipe.DeletedByUserId = null;
                }

                recipe.UpdatedAt = now;
                recipe.UpdatedByUserId = actorUserId;
            }

            // ✅ IMPORTANTE: Mapear header ANTES del SaveChanges que inserta/actualiza Recipe
            MapRecipeHeader(recipe, parsed, requester, advisor);

            // ✅ Hardening: nunca permitir null/empty en Status (DB: NOT NULL)
            recipe.Status = string.IsNullOrWhiteSpace(recipe.Status) ? "ABIERTA" : recipe.Status.Trim();

            // Guardar receta (si es nueva, se inserta acá con Status ya seteado)
            await _db.SaveChangesAsync();

            // Soft delete de hijos actuales + alta de nuevos
            SoftDeleteChildren(recipe, now, actorUserId);
            await AddChildrenFromParsedAsync(recipe, parsed, now, actorUserId);

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

            // Buscar SOLO el Id (rápido)
            var requesterId = await _db.Requesters
                .Where(x => x.TaxId == parsed.RequesterTaxId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            // No existe (ni activo ni soft-deleted)
            if (requesterId == 0)
            {
                var requesterNew = new Requester
                {
                    LegalName = parsed.RequesterName ?? "",
                    TaxId = parsed.RequesterTaxId,
                    Address = null,
                    Contact = null,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };

                _db.Requesters.Add(requesterNew);
                return requesterNew;
            }

            // Existe: traer entidad completa (incluye soft-deleted)
            var requester = await _db.Requesters
                .IgnoreQueryFilters()
                .FirstAsync(x => x.Id == requesterId);

            // Revivir si estaba soft-deleted
            if (requester.DeletedAt != null)
            {
                requester.DeletedAt = null;
                requester.DeletedByUserId = null;
            }

            // Actualizar datos
            if (!string.IsNullOrWhiteSpace(parsed.RequesterName))
                requester.LegalName = parsed.RequesterName;

            requester.UpdatedAt = now;
            requester.UpdatedByUserId = actorUserId;

            return requester;
        }

        private async Task<Advisor> UpsertAdvisorAsync(ParsedRecipe parsed, long actorUserId, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(parsed.AdvisorLicense))
                throw new InvalidOperationException("El PDF no trae matrícula (M.P.) del asesor.");

            var advisor = await _db.Advisors
                .IgnoreQueryFilters()
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
                // Revivir si estaba soft-deleted
                if (advisor.DeletedAt != null)
                {
                    advisor.DeletedAt = null;
                    advisor.DeletedByUserId = null;
                }

                if (!string.IsNullOrWhiteSpace(parsed.AdvisorName))
                    advisor.FullName = parsed.AdvisorName;

                advisor.UpdatedAt = now;
                advisor.UpdatedByUserId = actorUserId;
            }

            return advisor;
        }

        private async Task<Product> UpsertProductAsync(ParsedProduct parsedProduct, long actorUserId, DateTime now)
        {
            var senasa = parsedProduct.SenasaRegistry?.Trim();
            var name = (parsedProduct.ProductName ?? "").Trim();
            var tox = parsedProduct.ToxicologicalClass?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("El PDF trae un producto sin nombre.");

            Product? product;

            if (!string.IsNullOrWhiteSpace(senasa))
            {
                product = await _db.Products
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.SenasaRegistry == senasa);
            }
            else
            {
                product = await _db.Products
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x =>
                        x.SenasaRegistry == null &&
                        x.ProductName == name &&
                        (x.ToxicologicalClass ?? "") == (tox ?? ""));
            }

            if (product is null)
            {
                product = new Product
                {
                    SenasaRegistry = string.IsNullOrWhiteSpace(senasa) ? null : senasa,
                    ProductName = name,
                    ToxicologicalClass = tox,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };

                _db.Products.Add(product);
                return product;
            }

            if (product.DeletedAt != null)
            {
                product.DeletedAt = null;
                product.DeletedByUserId = null;
            }

            product.ProductName = name;
            product.ToxicologicalClass = tox;
            product.UpdatedAt = now;
            product.UpdatedByUserId = actorUserId;

            return product;
        }


        private static void MapRecipeHeader(Recipe recipe, ParsedRecipe parsed, Requester requester, Advisor advisor)
        {
            // ✅ parsed.Status puede venir ok, pero nunca lo dejamos null
            recipe.Status = parsed.Status;

            // En entidades Recipe estos campos son DateTime y EF los persiste como DATE
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

            // Estos 3 existen en tu tabla Recipes (y ya los mapeaste en DbContext)
            recipe.MachinePlate = parsed.MachinePlate;
            recipe.MachineLegalName = parsed.MachineLegalName;
            recipe.MachineType = parsed.MachineType;

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
                if (p.DeletedAt != null) continue;
                p.DeletedAt = now;
                p.DeletedByUserId = actorUserId;
            }

            foreach (var lot in recipe.Lots)
            {
                if (lot.DeletedAt == null)
                {
                    lot.DeletedAt = now;
                    lot.DeletedByUserId = actorUserId;
                }

                foreach (var v in lot.Vertices)
                {
                    if (v.DeletedAt != null) continue;
                    v.DeletedAt = now;
                }
            }

            foreach (var sp in recipe.SensitivePoints)
            {
                if (sp.DeletedAt != null) continue;
                sp.DeletedAt = now;
                sp.DeletedByUserId = actorUserId;
            }
        }

        private async Task AddChildrenFromParsedAsync(Recipe recipe, ParsedRecipe parsed, DateTime now, long actorUserId)
        {
            // Productos (RecipeProducts) -> ahora referencian catálogo Products
            foreach (var p in parsed.Products)
            {
                var product = await UpsertProductAsync(p, actorUserId, now);

                recipe.Products.Add(new RecipeProduct
                {
                    Product = product,              // o ProductId = product.Id
                    ProductType = p.ProductType,

                    // 👇👇👇 IMPORTANTE  (NOT NULL en ProductName)
                    ProductName = product.ProductName,
                    SenasaRegistry = product.SenasaRegistry,
                    ToxicologicalClass = product.ToxicologicalClass,

                    DoseValue = p.DoseValue,
                    DoseUnit = p.DoseUnit,
                    DosePerUnit = p.DosePerUnit,
                    TotalValue = p.TotalValue,
                    TotalUnit = p.TotalUnit,

                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                });
            }

            // Lotes + vértices
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
                        CreatedAt = now,
                    });
                }

                recipe.Lots.Add(lot);
            }

            // Si más adelante parseás puntos sensibles, se agregan acá.
            // foreach (var sp in parsed.SensitivePoints) ...
        }
    }
}

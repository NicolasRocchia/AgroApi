using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly AgroDbContext _context;

        public RecipeService(AgroDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<RecipeListItemDto>> GetRecipesAsync(RecipeQueryRequest request)
        {
            request.Validate();

            var query = _context.Recipes
                .Include(r => r.Requester)
                .Include(r => r.Advisor)
                .Include(r => r.Products)
                .Include(r => r.Lots)
                .Include(r => r.AssignedMunicipality)
                .AsQueryable();

            // Aplicar filtros
            query = ApplyFilters(query, request);

            // Aplicar ordenamiento
            query = ApplySorting(query, request);

            // Contar total
            var totalItems = await query.CountAsync();

            // Paginar y proyectar
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new RecipeListItemDto
                {
                    Id = r.Id,
                    RfdNumber = r.RfdNumber,
                    Status = r.Status,
                    IssueDate = r.IssueDate,
                    PossibleStartDate = r.PossibleStartDate,
                    ExpirationDate = r.ExpirationDate,
                    RequesterName = r.Requester.LegalName,
                    AdvisorName = r.Advisor.FullName,
                    Crop = r.Crop,
                    ApplicationType = r.ApplicationType,
                    UnitSurfaceHa = r.UnitSurfaceHa,
                    ProductsCount = r.Products.Count,
                    LotsCount = r.Lots.Count,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    AssignedMunicipalityName = r.AssignedMunicipality != null ? r.AssignedMunicipality.Name : null
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            return new PagedResponse<RecipeListItemDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            };
        }

        private IQueryable<Domain.Entities.Recipe> ApplyFilters(
            IQueryable<Domain.Entities.Recipe> query,
            RecipeQueryRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(r => r.Status == request.Status);

            if (request.RfdNumber.HasValue)
                query = query.Where(r => r.RfdNumber == request.RfdNumber.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchLower = request.SearchText.ToLower();
                query = query.Where(r =>
                    r.Requester.LegalName.ToLower().Contains(searchLower) ||
                    r.Advisor.FullName.ToLower().Contains(searchLower) ||
                    (r.Crop != null && r.Crop.ToLower().Contains(searchLower)) ||
                    (r.Diagnosis != null && r.Diagnosis.ToLower().Contains(searchLower))
                );
            }

            if (request.IssueDateFrom.HasValue)
                query = query.Where(r => r.IssueDate >= request.IssueDateFrom.Value);

            if (request.IssueDateTo.HasValue)
                query = query.Where(r => r.IssueDate <= request.IssueDateTo.Value);

            if (request.ExpirationDateFrom.HasValue)
                query = query.Where(r => r.ExpirationDate >= request.ExpirationDateFrom.Value);

            if (request.ExpirationDateTo.HasValue)
                query = query.Where(r => r.ExpirationDate <= request.ExpirationDateTo.Value);

            if (request.RequesterId.HasValue)
                query = query.Where(r => r.RequesterId == request.RequesterId.Value);

            if (request.AdvisorId.HasValue)
                query = query.Where(r => r.AdvisorId == request.AdvisorId.Value);

            if (!string.IsNullOrWhiteSpace(request.Crop))
                query = query.Where(r => r.Crop == request.Crop);

            if (!string.IsNullOrWhiteSpace(request.ApplicationType))
                query = query.Where(r => r.ApplicationType == request.ApplicationType);

            if (request.CreatedByUserId.HasValue)
                query = query.Where(r => r.CreatedByUserId == request.CreatedByUserId.Value);

            if (request.MunicipalityId.HasValue)
                query = query.Where(r => r.AssignedMunicipalityId == request.MunicipalityId.Value);

            return query;
        }

        private IQueryable<Domain.Entities.Recipe> ApplySorting(
            IQueryable<Domain.Entities.Recipe> query,
            RecipeQueryRequest request)
        {
            return request.SortBy.ToLower() switch
            {
                "rfdnumber" => request.SortDescending
                    ? query.OrderByDescending(r => r.RfdNumber)
                    : query.OrderBy(r => r.RfdNumber),
                "status" => request.SortDescending
                    ? query.OrderByDescending(r => r.Status)
                    : query.OrderBy(r => r.Status),
                "issuedate" => request.SortDescending
                    ? query.OrderByDescending(r => r.IssueDate)
                    : query.OrderBy(r => r.IssueDate),
                "expirationdate" => request.SortDescending
                    ? query.OrderByDescending(r => r.ExpirationDate)
                    : query.OrderBy(r => r.ExpirationDate),
                "requester" => request.SortDescending
                    ? query.OrderByDescending(r => r.Requester.LegalName)
                    : query.OrderBy(r => r.Requester.LegalName),
                "advisor" => request.SortDescending
                    ? query.OrderByDescending(r => r.Advisor.FullName)
                    : query.OrderBy(r => r.Advisor.FullName),
                _ => request.SortDescending
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
            };
        }

        public async Task ChangeStatusAsync(long recipeId, string newStatus, long userId)
        {
            var validStatuses = new[] { "ABIERTA", "PENDIENTE", "APROBADA", "RECHAZADA", "OBSERVADA", "CERRADA", "ANULADA" };
            newStatus = newStatus.ToUpper().Trim();

            if (!validStatuses.Contains(newStatus))
                throw new ArgumentException($"Estado inválido: {newStatus}. Los estados válidos son: {string.Join(", ", validStatuses)}");

            var recipe = await _context.Recipes.FindAsync(recipeId)
                ?? throw new InvalidOperationException($"No se encontró la receta con ID {recipeId}.");

            // Validar transiciones permitidas
            var allowedTransitions = new Dictionary<string, string[]>
            {
                { "ABIERTA", new[] { "CERRADA", "ANULADA", "PENDIENTE" } },
                { "APROBADA", new[] { "CERRADA", "ANULADA" } },
                { "RECHAZADA", new[] { "ANULADA" } },
            };

            if (!allowedTransitions.ContainsKey(recipe.Status) || !allowedTransitions[recipe.Status].Contains(newStatus))
                throw new InvalidOperationException($"No se puede cambiar de {recipe.Status} a {newStatus}.");

            recipe.Status = newStatus;
            recipe.UpdatedAt = DateTime.UtcNow;
            recipe.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        public async Task<RecipeDetailDto?> GetRecipeByIdAsync(long id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Requester)
                .Include(r => r.Advisor)
                .Include(r => r.Products)
                    .ThenInclude(p => p.Product)
                .Include(r => r.Lots)
                    .ThenInclude(l => l.Vertices.OrderBy(v => v.Order))
                .Include(r => r.SensitivePoints)
                .Include(r => r.AssignedMunicipality)
                .Include(r => r.ReviewLogs)
                    .ThenInclude(rl => rl.Municipality)
                .Include(r => r.ReviewLogs)
                    .ThenInclude(rl => rl.TargetMunicipality)
                .Include(r => r.Messages)
                    .ThenInclude(m => m.Sender)
                        .ThenInclude(s => s.UserRoles)
                            .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return null;

            // Obtener nombres de usuarios para auditoría
            string? createdByUserName = null;
            string? updatedByUserName = null;

            if (recipe.CreatedByUserId.HasValue)
            {
                createdByUserName = await _context.Users
                    .Where(u => u.Id == recipe.CreatedByUserId.Value)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();
            }

            if (recipe.UpdatedByUserId.HasValue)
            {
                updatedByUserName = await _context.Users
                    .Where(u => u.Id == recipe.UpdatedByUserId.Value)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();
            }

            return new RecipeDetailDto
            {
                Id = recipe.Id,
                RfdNumber = recipe.RfdNumber,
                Status = recipe.Status,

                IssueDate = recipe.IssueDate,
                PossibleStartDate = recipe.PossibleStartDate,
                RecommendedDate = recipe.RecommendedDate,
                ExpirationDate = recipe.ExpirationDate,

                Requester = new RequesterDto
                {
                    Id = recipe.Requester.Id,
                    LegalName = recipe.Requester.LegalName,
                    TaxId = recipe.Requester.TaxId,
                    Address = recipe.Requester.Address,
                    Contact = recipe.Requester.Contact
                },

                Advisor = new AdvisorDto
                {
                    Id = recipe.Advisor.Id,
                    FullName = recipe.Advisor.FullName,
                    LicenseNumber = recipe.Advisor.LicenseNumber,
                    Contact = recipe.Advisor.Contact
                },

                ApplicationType = recipe.ApplicationType,
                Crop = recipe.Crop,
                Diagnosis = recipe.Diagnosis,
                Treatment = recipe.Treatment,
                MachineToUse = recipe.MachineToUse,
                UnitSurfaceHa = recipe.UnitSurfaceHa,

                TempMin = recipe.TempMin,
                TempMax = recipe.TempMax,
                HumidityMin = recipe.HumidityMin,
                HumidityMax = recipe.HumidityMax,
                WindMinKmh = recipe.WindMinKmh,
                WindMaxKmh = recipe.WindMaxKmh,
                WindDirection = recipe.WindDirection,

                MachinePlate = recipe.MachinePlate,
                MachineLegalName = recipe.MachineLegalName,
                MachineType = recipe.MachineType,

                Notes = recipe.Notes,

                Products = recipe.Products.Select(p => new RecipeProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    SenasaRegistry = p.SenasaRegistry,
                    ToxicologicalClass = p.ToxicologicalClass,
                    ProductType = p.ProductType,
                    DoseValue = p.DoseValue,
                    DoseUnit = p.DoseUnit,
                    DosePerUnit = p.DosePerUnit,
                    TotalValue = p.TotalValue,
                    TotalUnit = p.TotalUnit
                }).ToList(),

                Lots = recipe.Lots.Select(l => new RecipeLotDto
                {
                    Id = l.Id,
                    LotName = l.LotName,
                    Locality = l.Locality,
                    Department = l.Department,
                    SurfaceHa = l.SurfaceHa,
                    Vertices = l.Vertices.Select(v => new RecipeLotVertexDto
                    {
                        Id = v.Id,
                        Order = v.Order,
                        Latitude = v.Latitude,
                        Longitude = v.Longitude
                    }).ToList()
                }).ToList(),

                SensitivePoints = recipe.SensitivePoints.Select(sp => new RecipeSensitivePointDto
                {
                    Id = sp.Id,
                    Name = sp.Name,
                    Type = sp.Type,
                    Latitude = sp.Latitude,
                    Longitude = sp.Longitude,
                    Locality = sp.Locality,
                    Department = sp.Department
                }).ToList(),

                CreatedAt = recipe.CreatedAt,
                CreatedByUserId = recipe.CreatedByUserId,
                CreatedByUserName = createdByUserName,
                UpdatedAt = recipe.UpdatedAt,
                UpdatedByUserId = recipe.UpdatedByUserId,
                UpdatedByUserName = updatedByUserName,

                // Municipal
                AssignedMunicipalityId = recipe.AssignedMunicipalityId,
                AssignedMunicipalityName = recipe.AssignedMunicipality?.Name,
                AssignedAt = recipe.AssignedAt,
                ReviewLogs = recipe.ReviewLogs
                    .OrderByDescending(rl => rl.CreatedAt)
                    .Select(rl => new Contracts.Responses.RecipeReviewLogDto
                    {
                        Id = rl.Id,
                        Action = rl.Action,
                        MunicipalityName = rl.Municipality.Name,
                        TargetMunicipalityName = rl.TargetMunicipality?.Name,
                        Observation = rl.Observation,
                        CreatedAt = rl.CreatedAt,
                        CreatedByUserName = null // se resuelve abajo
                    }).ToList(),
                Messages = recipe.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new Contracts.Responses.RecipeMessageDto
                    {
                        Id = m.Id,
                        SenderUserId = m.SenderUserId,
                        SenderName = m.Sender.UserName,
                        SenderRole = m.Sender.UserRoles
                            .Select(ur => ur.Role.Name)
                            .FirstOrDefault() ?? "Desconocido",
                        Message = m.Message,
                        CreatedAt = m.CreatedAt
                    }).ToList()
            };
        }

        public async Task<Domain.Entities.Municipality?> GetMunicipalityByUserIdAsync(long userId)
        {
            return await _context.Municipalities
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }
    }
}

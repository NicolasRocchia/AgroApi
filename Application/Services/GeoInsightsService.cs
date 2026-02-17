using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace APIAgroConnect.Application.Services
{
    public class GeoInsightsService : IGeoInsightsService
    {
        private readonly AgroDbContext _db;

        // Mapeo de clases toxicológicas a scores numéricos (menor = más peligroso)
        private static readonly Dictionary<string, int> ToxScoreMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Ia", 1 }, { "Clase Ia", 1 }, { "Ia - Extremadamente peligroso", 1 },
            { "Ib", 2 }, { "Clase Ib", 2 }, { "Ib - Altamente peligroso", 2 },
            { "II", 3 }, { "Clase II", 3 }, { "II - Moderadamente peligroso", 3 },
            { "III", 4 }, { "Clase III", 4 }, { "III - Ligeramente peligroso", 4 },
            { "IV", 5 }, { "Clase IV", 5 }, { "IV - Normalmente no peligroso", 5 },
        };

        // Radio en km para considerar "cercano" a un punto sensible
        private const double NearbyRadiusKm = 1.0;

        public GeoInsightsService(AgroDbContext db)
        {
            _db = db;
        }

        public async Task<GeoInsightsResponse> GetGeoInsightsAsync(long? municipalityId, GeoInsightsRequest request)
        {
            // 1. Obtener recetas (filtrar por municipio solo si se especifica)
            var query = _db.Recipes
                .Include(r => r.Lots).ThenInclude(l => l.Vertices)
                .Include(r => r.Products)
                .Include(r => r.SensitivePoints)
                .Include(r => r.Advisor)
                .Include(r => r.Requester)
                .AsQueryable();

            if (municipalityId.HasValue)
                query = query.Where(r => r.AssignedMunicipalityId == municipalityId.Value);

            // Aplicar filtros
            if (request.DateFrom.HasValue)
                query = query.Where(r => r.IssueDate >= request.DateFrom.Value);
            if (request.DateTo.HasValue)
                query = query.Where(r => r.IssueDate <= request.DateTo.Value);
            if (!string.IsNullOrWhiteSpace(request.Crop))
                query = query.Where(r => r.Crop != null && r.Crop.ToLower().Contains(request.Crop.ToLower()));
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(r => r.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.ToxClass))
                query = query.Where(r => r.Products.Any(p => p.ToxicologicalClass != null && p.ToxicologicalClass.Contains(request.ToxClass)));
            if (!string.IsNullOrWhiteSpace(request.ProductName))
                query = query.Where(r => r.Products.Any(p => p.ProductName.ToLower().Contains(request.ProductName.ToLower())));
            if (!string.IsNullOrWhiteSpace(request.AdvisorName))
                query = query.Where(r => r.Advisor.FullName.ToLower().Contains(request.AdvisorName.ToLower()));

            var recipes = await query.AsNoTracking().ToListAsync();

            // 2. Construir aplicaciones geo
            var applications = new List<GeoApplicationDto>();
            var allSensitivePoints = new Dictionary<long, GeoSensitivePointDto>();

            foreach (var recipe in recipes)
            {
                var products = recipe.Products.Select(p => new GeoProductDto
                {
                    ProductName = p.ProductName,
                    ToxicologicalClass = p.ToxicologicalClass,
                    SenasaRegistry = p.SenasaRegistry
                }).ToList();

                var maxToxClass = GetMaxToxClass(recipe.Products);
                var toxScore = GetToxScore(maxToxClass);

                foreach (var lot in recipe.Lots.Where(l => l.Vertices.Any()))
                {
                    var vertices = lot.Vertices
                        .OrderBy(v => v.Order)
                        .Select(v => new GeoVertexDto { Lat = v.Latitude, Lng = v.Longitude })
                        .ToList();

                    var centerLat = vertices.Average(v => v.Lat);
                    var centerLng = vertices.Average(v => v.Lng);

                    applications.Add(new GeoApplicationDto
                    {
                        RecipeId = recipe.Id,
                        RfdNumber = recipe.RfdNumber,
                        Status = recipe.Status,
                        IssueDate = recipe.IssueDate,
                        LotId = lot.Id,
                        LotName = lot.LotName,
                        Locality = lot.Locality,
                        Department = lot.Department,
                        SurfaceHa = lot.SurfaceHa,
                        Vertices = vertices,
                        CenterLat = centerLat,
                        CenterLng = centerLng,
                        Products = products,
                        Crop = recipe.Crop,
                        AdvisorName = recipe.Advisor?.FullName,
                        RequesterName = recipe.Requester?.LegalName,
                        MaxToxClass = maxToxClass,
                        ToxScore = toxScore
                    });
                }

                // Recopilar puntos sensibles (deduplicar por ID)
                foreach (var sp in recipe.SensitivePoints)
                {
                    if (!allSensitivePoints.ContainsKey(sp.Id))
                    {
                        allSensitivePoints[sp.Id] = new GeoSensitivePointDto
                        {
                            Id = sp.Id,
                            Name = sp.Name,
                            Type = sp.Type,
                            Latitude = sp.Latitude,
                            Longitude = sp.Longitude,
                            Locality = sp.Locality,
                            NearbyApplicationsCount = 0,
                            NearbyHighToxCount = 0
                        };
                    }
                }
            }

            // 3. Calcular cercanía de aplicaciones a puntos sensibles
            // TODO: Descomentar cuando se active el sistema de alertas
            // foreach (var sp in allSensitivePoints.Values)
            // {
            //     foreach (var app in applications)
            //     {
            //         var distKm = HaversineKm(
            //             (double)sp.Latitude, (double)sp.Longitude,
            //             (double)app.CenterLat, (double)app.CenterLng);
            //
            //         if (distKm <= NearbyRadiusKm)
            //         {
            //             sp.NearbyApplicationsCount++;
            //             if (app.ToxScore <= 2) // Clase Ia o Ib
            //                 sp.NearbyHighToxCount++;
            //         }
            //     }
            // }

            // 4. Generar alertas (desactivado por ahora)
            // var alerts = GenerateAlerts(applications, allSensitivePoints.Values.ToList());
            var alerts = new List<GeoAlertDto>();

            // 5. KPIs
            var kpis = new GeoInsightsKpis
            {
                TotalApplications = applications.Count,
                TotalHectares = applications.Where(a => a.SurfaceHa.HasValue).Sum(a => a.SurfaceHa!.Value),
                UniqueProducts = applications.SelectMany(a => a.Products).Select(p => p.ProductName).Distinct().Count(),
                HighToxApplications = applications.Count(a => a.ToxScore <= 2),
                // TODO: Activar cuando se habiliten alertas
                // SensitivePointsAtRisk = allSensitivePoints.Values.Count(sp => sp.NearbyHighToxCount > 0),
                SensitivePointsAtRisk = 0,
                UniqueAdvisors = applications.Where(a => a.AdvisorName != null).Select(a => a.AdvisorName).Distinct().Count()
            };

            // 6. Filtros disponibles (para poblar los selects del frontend)
            var allRecipesQuery = _db.Recipes
                .Include(r => r.Products)
                .Include(r => r.Advisor)
                .AsNoTracking()
                .AsQueryable();

            if (municipalityId.HasValue)
                allRecipesQuery = allRecipesQuery.Where(r => r.AssignedMunicipalityId == municipalityId.Value);

            var allRecipesBase = await allRecipesQuery.ToListAsync();

            var availableFilters = new GeoFiltersAvailable
            {
                Crops = allRecipesBase.Where(r => !string.IsNullOrEmpty(r.Crop)).Select(r => r.Crop!).Distinct().OrderBy(c => c).ToList(),
                ToxClasses = allRecipesBase.SelectMany(r => r.Products).Where(p => !string.IsNullOrEmpty(p.ToxicologicalClass)).Select(p => p.ToxicologicalClass!).Distinct().OrderBy(t => t).ToList(),
                Products = allRecipesBase.SelectMany(r => r.Products).Select(p => p.ProductName).Distinct().OrderBy(p => p).ToList(),
                Advisors = allRecipesBase.Where(r => r.Advisor != null).Select(r => r.Advisor.FullName).Distinct().OrderBy(a => a).ToList(),
                EarliestDate = allRecipesBase.Any() ? allRecipesBase.Min(r => r.IssueDate) : null,
                LatestDate = allRecipesBase.Any() ? allRecipesBase.Max(r => r.IssueDate) : null
            };

            return new GeoInsightsResponse
            {
                Applications = applications,
                SensitivePoints = allSensitivePoints.Values.ToList(),
                Kpis = kpis,
                Alerts = alerts,
                AvailableFilters = availableFilters
            };
        }

        private string? GetMaxToxClass(ICollection<Domain.Entities.RecipeProduct> products)
        {
            string? worst = null;
            int worstScore = int.MaxValue;

            foreach (var p in products)
            {
                if (string.IsNullOrEmpty(p.ToxicologicalClass)) continue;
                var score = GetToxScore(p.ToxicologicalClass);
                if (score < worstScore)
                {
                    worstScore = score;
                    worst = p.ToxicologicalClass;
                }
            }

            return worst;
        }

        private static int GetToxScore(string? toxClass)
        {
            if (string.IsNullOrEmpty(toxClass)) return 99;

            // Buscar match exacto primero
            if (ToxScoreMap.TryGetValue(toxClass, out var score))
                return score;

            // Buscar match parcial (e.g., "Clase Ia - Extremadamente peligroso" contiene "Ia")
            var normalized = toxClass.Trim();
            if (normalized.Contains("Ia", StringComparison.OrdinalIgnoreCase)) return 1;
            if (normalized.Contains("Ib", StringComparison.OrdinalIgnoreCase)) return 2;
            if (normalized.Contains("II", StringComparison.OrdinalIgnoreCase) && !normalized.Contains("III", StringComparison.OrdinalIgnoreCase)) return 3;
            if (normalized.Contains("III", StringComparison.OrdinalIgnoreCase) && !normalized.Contains("IV", StringComparison.OrdinalIgnoreCase)) return 4;
            if (normalized.Contains("IV", StringComparison.OrdinalIgnoreCase)) return 5;

            return 99; // desconocido
        }

        private List<GeoAlertDto> GenerateAlerts(List<GeoApplicationDto> applications, List<GeoSensitivePointDto> sensitivePoints)
        {
            var alerts = new List<GeoAlertDto>();

            // Alerta: puntos sensibles con productos alta toxicidad cercanos
            foreach (var sp in sensitivePoints.Where(s => s.NearbyHighToxCount > 0))
            {
                alerts.Add(new GeoAlertDto
                {
                    Level = "critical",
                    Title = $"⚠️ Productos Clase I/II cerca de {sp.Name}",
                    Description = $"Se detectaron {sp.NearbyHighToxCount} aplicaciones con productos de alta toxicidad a menos de {NearbyRadiusKm}km de \"{sp.Name}\" ({sp.Type ?? "punto sensible"}).",
                    Latitude = sp.Latitude,
                    Longitude = sp.Longitude
                });
            }

            // Alerta: concentración de aplicaciones en misma zona
            var grouped = applications
                .GroupBy(a => new { Lat = Math.Round((double)a.CenterLat, 2), Lng = Math.Round((double)a.CenterLng, 2) })
                .Where(g => g.Count() >= 5)
                .OrderByDescending(g => g.Count());

            foreach (var cluster in grouped)
            {
                alerts.Add(new GeoAlertDto
                {
                    Level = "warning",
                    Title = $"📍 Alta densidad de aplicaciones",
                    Description = $"Se registraron {cluster.Count()} aplicaciones concentradas en la zona ({cluster.Key.Lat:F2}, {cluster.Key.Lng:F2}). Verificar posible sobreexposición.",
                    Latitude = (decimal)cluster.Key.Lat,
                    Longitude = (decimal)cluster.Key.Lng
                });
            }

            // Alerta: asesor con muchas aplicaciones de alta toxicidad
            var advisorHighTox = applications
                .Where(a => a.ToxScore <= 2 && !string.IsNullOrEmpty(a.AdvisorName))
                .GroupBy(a => a.AdvisorName)
                .Where(g => g.Count() >= 3)
                .OrderByDescending(g => g.Count());

            foreach (var group in advisorHighTox)
            {
                alerts.Add(new GeoAlertDto
                {
                    Level = "warning",
                    Title = $"👨‍🔬 Asesor con frecuentes prescripciones tóxicas",
                    Description = $"El asesor \"{group.Key}\" tiene {group.Count()} recetas con productos Clase I/II. Considerar revisión.",
                });
            }

            return alerts.OrderBy(a => a.Level == "critical" ? 0 : a.Level == "warning" ? 1 : 2).ToList();
        }

        /// <summary>Distancia Haversine en kilómetros</summary>
        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
    }
}

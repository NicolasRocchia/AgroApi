using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly AgroDbContext _db;

        public InsightsService(AgroDbContext db)
        {
            _db = db;
        }

        public async Task<InsightsDto> GetInsightsAsync()
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);
            var twelveMonthsAgo = now.AddMonths(-12);

            var totalRecipes = await _db.Recipes.CountAsync();
            var totalUsers = await _db.Users.CountAsync();
            var totalProducts = await _db.Products.CountAsync();
            var recipesLastMonth = await _db.Recipes
                .Where(r => r.CreatedAt >= oneMonthAgo)
                .CountAsync();

            var recipesByMonth = await _db.Recipes
                .Where(r => r.CreatedAt >= twelveMonthsAgo)
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var monthlyData = new List<MonthlyCountDto>();
            for (int i = 11; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var match = recipesByMonth
                    .FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);

                monthlyData.Add(new MonthlyCountDto
                {
                    Month = date.ToString("MMM yy"),
                    Count = match?.Count ?? 0
                });
            }

            var recipesByStatus = await _db.Recipes
                .GroupBy(r => r.Status)
                .Select(g => new NameCountDto
                {
                    Name = g.Key ?? "Sin estado",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var topProducts = await _db.RecipeProducts
                .GroupBy(rp => rp.ProductName)
                .Select(g => new NameCountDto
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var byToxClass = await _db.RecipeProducts
                .Where(rp => !string.IsNullOrEmpty(rp.ToxicologicalClass))
                .GroupBy(rp => rp.ToxicologicalClass!)
                .Select(g => new NameCountDto
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var topRequesters = await _db.Recipes
                .Include(r => r.Requester)
                .GroupBy(r => r.Requester.LegalName)
                .Select(g => new NameCountDto
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var topAdvisors = await _db.Recipes
                .Include(r => r.Advisor)
                .GroupBy(r => r.Advisor.FullName)
                .Select(g => new NameCountDto
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return new InsightsDto
            {
                TotalRecipes = totalRecipes,
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                RecipesLastMonth = recipesLastMonth,
                RecipesByMonth = monthlyData,
                RecipesByStatus = recipesByStatus,
                TopProducts = topProducts,
                ByToxicologicalClass = byToxClass,
                TopRequesters = topRequesters,
                TopAdvisors = topAdvisors
            };
        }
    }
}

namespace APIAgroConnect.Contracts.Responses
{
    public class InsightsDto
    {
        // KPIs
        public int TotalRecipes { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int RecipesLastMonth { get; set; }

        // Recetas por mes (últimos 12 meses)
        public List<MonthlyCountDto> RecipesByMonth { get; set; } = new();

        // Recetas por estado
        public List<NameCountDto> RecipesByStatus { get; set; } = new();

        // Top 10 productos más usados
        public List<NameCountDto> TopProducts { get; set; } = new();

        // Distribución por clase toxicológica
        public List<NameCountDto> ByToxicologicalClass { get; set; } = new();

        // Top 10 solicitantes
        public List<NameCountDto> TopRequesters { get; set; } = new();

        // Top 10 asesores
        public List<NameCountDto> TopAdvisors { get; set; } = new();
    }

    public class MonthlyCountDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class NameCountDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}

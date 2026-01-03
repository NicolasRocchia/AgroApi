namespace APIAgroConnect.Domain.Entities
{
    public class RecipeSensitivePoint : BaseAuditableEntity
    {
        public long RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public string? Locality { get; set; }
        public string? Department { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}

namespace APIAgroConnect.Domain.Entities
{
    public class RecipeProduct : BaseAuditableEntity
    {
        public long Id { get; set; }
        public long RecipeId { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string? ProductType { get; set; }

        public decimal? DoseValue { get; set; }
        public string? DoseUnit { get; set; }
        public string? DosePerUnit { get; set; }

        public decimal? TotalValue { get; set; }
        public string? TotalUnit { get; set; }
        public string ProductName { get; set; } = null!;
        public string? SenasaRegistry { get; set; }
        public string? ToxicologicalClass { get; set; }


        public Recipe Recipe { get; set; } = null!;
    }
}

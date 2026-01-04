namespace APIAgroConnect.Domain.Entities
{
    public class Recipe : BaseAuditableEntity
    {
        public long RfdNumber { get; set; }
        public string Status { get; set; } = null!;

        public DateTime IssueDate { get; set; }
        public DateTime? PossibleStartDate { get; set; }
        public DateTime? RecommendedDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public long RequesterId { get; set; }
        public Requester Requester { get; set; } = null!;

        public long AdvisorId { get; set; }
        public Advisor Advisor { get; set; } = null!;

        public string? ApplicationType { get; set; }
        public string? Crop { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? MachineToUse { get; set; }

        public decimal? UnitSurfaceHa { get; set; }

        public decimal? TempMin { get; set; }
        public decimal? TempMax { get; set; }
        public decimal? HumidityMin { get; set; }
        public decimal? HumidityMax { get; set; }
        public decimal? WindMinKmh { get; set; }
        public decimal? WindMaxKmh { get; set; }
        public string? WindDirection { get; set; }

        public string? Notes { get; set; }

        public ICollection<RecipeProduct> Products { get; set; } = new List<RecipeProduct>();
        public ICollection<RecipeLot> Lots { get; set; } = new List<RecipeLot>();
        public ICollection<RecipeSensitivePoint> SensitivePoints { get; set; } = new List<RecipeSensitivePoint>();
    }
}

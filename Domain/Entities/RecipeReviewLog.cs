namespace APIAgroConnect.Domain.Entities
{
    public class RecipeReviewLog
    {
        public long Id { get; set; }
        public long RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public long MunicipalityId { get; set; }
        public Municipality Municipality { get; set; } = null!;

        public string Action { get; set; } = null!; // APROBADA, RECHAZADA, OBSERVADA, REDIRIGIDA

        public long? TargetMunicipalityId { get; set; }
        public Municipality? TargetMunicipality { get; set; }

        public string? Observation { get; set; }

        public DateTime CreatedAt { get; set; }
        public long? CreatedByUserId { get; set; }
    }
}

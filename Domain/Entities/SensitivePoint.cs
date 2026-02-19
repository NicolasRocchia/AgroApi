namespace APIAgroConnect.Domain.Entities
{
    /// <summary>
    /// Tabla maestra de puntos sensibles (escuelas, hospitales, etc).
    /// Un punto sensible es único por nombre + coordenadas.
    /// Múltiples recetas pueden referenciar el mismo punto sensible.
    /// </summary>
    public class SensitivePoint : BaseAuditableEntity
    {
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public string? Locality { get; set; }
        public string? Department { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // N:M con Recipes via tabla puente
        public ICollection<RecipeSensitivePointMap> RecipeMappings { get; set; } = new List<RecipeSensitivePointMap>();
    }
}

namespace APIAgroConnect.Domain.Entities
{
    /// <summary>
    /// Tabla puente Recipe ↔ SensitivePoint (N:M).
    /// Registra qué puntos sensibles aparecen en cada receta.
    /// </summary>
    public class RecipeSensitivePointMap
    {
        public long RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public long SensitivePointId { get; set; }
        public SensitivePoint SensitivePoint { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}

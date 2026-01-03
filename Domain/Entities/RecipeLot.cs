using System.Collections.Generic;

namespace APIAgroConnect.Domain.Entities
{
    public class RecipeLot : BaseAuditableEntity
    {       
        public long RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public string LotName { get; set; } = null!;
        public string? Locality { get; set; }
        public string? Department { get; set; }
        public decimal? SurfaceHa { get; set; }

        public ICollection<RecipeLotVertex> Vertices { get; set; } = new List<RecipeLotVertex>();
    }
}

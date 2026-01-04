namespace APIAgroConnect.Domain.Entities
{
    public class Product
    {
        public long Id { get; set; }

        public string? SenasaRegistry { get; set; }          // nvarchar(50) null
        public string ProductName { get; set; } = null!;     // nvarchar(200) not null
        public string? ToxicologicalClass { get; set; }      // nvarchar(100) null

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long? CreatedByUserId { get; set; }
        public long? UpdatedByUserId { get; set; }
        public long? DeletedByUserId { get; set; }

        public ICollection<RecipeProduct> RecipeProducts { get; set; } = new List<RecipeProduct>();
    }
}

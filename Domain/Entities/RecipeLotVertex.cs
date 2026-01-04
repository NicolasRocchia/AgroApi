namespace APIAgroConnect.Domain.Entities
{
    public class RecipeLotVertex : BaseAuditableEntity
    {
        public long LotId { get; set; }
        public RecipeLot Lot { get; set; } = null!;

        public int Order { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public ICollection<RecipeLotVertex> Vertices { get; set; } = new List<RecipeLotVertex>();
    }
}

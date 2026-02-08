namespace APIAgroConnect.Domain.Entities
{
    public class RecipeLotVertex 
    {
        public long Id { get; set; }

        public long LotId { get; set; }
        public RecipeLot Lot { get; set; }

        public int Order { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

namespace APIAgroConnect.Contracts.Responses
{
    public class RecipeListItemDto
    {
        public long Id { get; set; }
        public long RfdNumber { get; set; }
        public string Status { get; set; } = null!;
        
        public DateTime IssueDate { get; set; }
        public DateTime? PossibleStartDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        public string RequesterName { get; set; } = null!;
        public string AdvisorName { get; set; } = null!;
        
        public string? Crop { get; set; }
        public string? ApplicationType { get; set; }
        public decimal? UnitSurfaceHa { get; set; }
        
        public int ProductsCount { get; set; }
        public int LotsCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

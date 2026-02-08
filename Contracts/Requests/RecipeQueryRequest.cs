namespace APIAgroConnect.Contracts.Requests
{
    public class RecipeQueryRequest
    {
        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Filtros
        public string? Status { get; set; }
        public long? RfdNumber { get; set; }
        public string? SearchText { get; set; }
        
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        
        public DateTime? ExpirationDateFrom { get; set; }
        public DateTime? ExpirationDateTo { get; set; }
        
        public long? RequesterId { get; set; }
        public long? AdvisorId { get; set; }
        
        public string? Crop { get; set; }
        public string? ApplicationType { get; set; }
        
        // Ordenamiento
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        
        // Validación
        public void Validate()
        {
            if (Page < 1) Page = 1;
            if (PageSize < 1) PageSize = 20;
            if (PageSize > 100) PageSize = 100;
        }
    }
}

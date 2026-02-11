namespace APIAgroConnect.Contracts.Responses
{
    public class MunicipalityDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Province { get; set; }
        public string? Department { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public double? DistanceKm { get; set; } // calculado al listar por cercanía
    }

    public class RecipeReviewLogDto
    {
        public long Id { get; set; }
        public string Action { get; set; } = null!;
        public string MunicipalityName { get; set; } = null!;
        public string? TargetMunicipalityName { get; set; }
        public string? Observation { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    public class RecipeMessageDto
    {
        public long Id { get; set; }
        public long SenderUserId { get; set; }
        public string SenderName { get; set; } = null!;
        public string SenderRole { get; set; } = null!; // "Aplicador" o "Municipio"
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}

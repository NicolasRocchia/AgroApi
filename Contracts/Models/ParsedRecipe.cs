namespace APIAgroConnect.Contracts.Models
{
    public sealed class ParsedRecipe
    {
        public long RfdNumber { get; set; }
        public string Status { get; set; } = "";
        public DateOnly IssueDate { get; set; }
        public DateOnly? PossibleStartDate { get; set; }
        public DateOnly? RecommendedDate { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public string RequesterName { get; set; } = "";
        public string RequesterTaxId { get; set; } = "";
        public string AdvisorName { get; set; } = "";
        public string AdvisorLicense { get; set; } = "";
        public string? ApplicationType { get; set; }
        public string? Crop { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? MachineToUse { get; set; }
        public string? MachinePlate { get; set; }
        public string? MachineLegalName { get; set; }
        public string? MachineType { get; set; }
        public decimal? UnitSurfaceHa { get; set; }
        public decimal? TempMin { get; set; }
        public decimal? TempMax { get; set; }
        public decimal? HumidityMin { get; set; }
        public decimal? HumidityMax { get; set; }
        public decimal? WindMinKmh { get; set; }
        public decimal? WindMaxKmh { get; set; }
        public string? WindDirection { get; set; }
        public string? Notes { get; set; }
        public List<ParsedProduct> Products { get; set; } = new();
        public List<ParsedLot> Lots { get; set; } = new();
        public List<ParsedSensitivePoint> SensitivePoints { get; set; } = new();
    }

    public sealed class ParsedProduct
    {
        public string? ProductType { get; set; }
        public string ProductName { get; set; } = "";
        public string? SenasaRegistry { get; set; }
        public string? ToxicologicalClass { get; set; }
        public decimal? DoseValue { get; set; }
        public string? DoseUnit { get; set; }
        public string? DosePerUnit { get; set; }
        public decimal? TotalValue { get; set; }
        public string? TotalUnit { get; set; }
    }

    public sealed class ParsedLot
    {
        public string LotName { get; set; } = "";
        public string? Locality { get; set; }
        public string? Department { get; set; }
        public decimal? SurfaceHa { get; set; }
        public List<ParsedVertex> Vertices { get; set; } = new();
    }

    public sealed class ParsedVertex
    {
        public int Order { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public sealed class ParsedSensitivePoint
    {
        public string Name { get; set; } = "";
        public string? Type { get; set; }
        public string? Locality { get; set; }
        public string? Department { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}

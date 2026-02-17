namespace APIAgroConnect.Contracts.Responses
{
    /// <summary>
    /// Respuesta del endpoint de geo-insights para el mapa de fiscalización municipal
    /// </summary>
    public class GeoInsightsResponse
    {
        /// <summary>Polígonos de lotes con metadata de aplicación</summary>
        public List<GeoApplicationDto> Applications { get; set; } = new();

        /// <summary>Puntos sensibles (escuelas, hospitales, etc.)</summary>
        public List<GeoSensitivePointDto> SensitivePoints { get; set; } = new();

        /// <summary>KPIs resumidos</summary>
        public GeoInsightsKpis Kpis { get; set; } = new();

        /// <summary>Alertas de zonas con actividad riesgosa</summary>
        public List<GeoAlertDto> Alerts { get; set; } = new();

        /// <summary>Filtros disponibles para el frontend</summary>
        public GeoFiltersAvailable AvailableFilters { get; set; } = new();
    }

    public class GeoApplicationDto
    {
        public long RecipeId { get; set; }
        public long RfdNumber { get; set; }
        public string Status { get; set; } = null!;
        public DateTime IssueDate { get; set; }

        // Lote
        public long LotId { get; set; }
        public string LotName { get; set; } = null!;
        public string? Locality { get; set; }
        public string? Department { get; set; }
        public decimal? SurfaceHa { get; set; }
        public List<GeoVertexDto> Vertices { get; set; } = new();
        public decimal CenterLat { get; set; }
        public decimal CenterLng { get; set; }

        // Productos aplicados en esta receta
        public List<GeoProductDto> Products { get; set; } = new();

        // Metadata de la receta
        public string? Crop { get; set; }
        public string? AdvisorName { get; set; }
        public string? RequesterName { get; set; }

        /// <summary>Clase toxicológica máxima (la peor) de los productos</summary>
        public string? MaxToxClass { get; set; }

        /// <summary>Score numérico de toxicidad (1=Ia, 2=Ib, 3=II, 4=III, 5=IV)</summary>
        public int ToxScore { get; set; }
    }

    public class GeoVertexDto
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }

    public class GeoProductDto
    {
        public string ProductName { get; set; } = null!;
        public string? ToxicologicalClass { get; set; }
        public string? SenasaRegistry { get; set; }
    }

    public class GeoSensitivePointDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Locality { get; set; }

        /// <summary>Cantidad de aplicaciones en un radio de 500m</summary>
        public int NearbyApplicationsCount { get; set; }

        /// <summary>Productos Clase I/II cercanos</summary>
        public int NearbyHighToxCount { get; set; }
    }

    public class GeoInsightsKpis
    {
        public int TotalApplications { get; set; }
        public decimal TotalHectares { get; set; }
        public int UniqueProducts { get; set; }
        public int HighToxApplications { get; set; }
        public int SensitivePointsAtRisk { get; set; }
        public int UniqueAdvisors { get; set; }
    }

    public class GeoAlertDto
    {
        public string Level { get; set; } = null!; // "critical", "warning", "info"
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class GeoFiltersAvailable
    {
        public List<string> Crops { get; set; } = new();
        public List<string> ToxClasses { get; set; } = new();
        public List<string> Products { get; set; } = new();
        public List<string> Advisors { get; set; } = new();
        public DateTime? EarliestDate { get; set; }
        public DateTime? LatestDate { get; set; }
    }
}

namespace APIAgroConnect.Contracts.Requests
{
    public class GeoInsightsRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Crop { get; set; }
        public string? ToxClass { get; set; }
        public string? ProductName { get; set; }
        public string? AdvisorName { get; set; }
        public string? Status { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace APIAgroConnect.Contracts.Requests
{
    public class AssignMunicipalityRequest
    {
        [Required(ErrorMessage = "El municipio es requerido.")]
        public long MunicipalityId { get; set; }
    }

    public class ReviewRecipeRequest
    {
        [Required(ErrorMessage = "La acción es requerida.")]
        public string Action { get; set; } = string.Empty; // APROBADA, RECHAZADA, OBSERVADA, REDIRIGIDA

        public string? Observation { get; set; }

        /// <summary>Solo para acción REDIRIGIDA</summary>
        public long? TargetMunicipalityId { get; set; }
    }

    public class SendRecipeMessageRequest
    {
        [Required(ErrorMessage = "El mensaje es requerido.")]
        [MaxLength(1000, ErrorMessage = "El mensaje no puede superar los 1000 caracteres.")]
        public string Message { get; set; } = string.Empty;
    }

    public class CreateMunicipalityRequest
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(150)]
        public string? Department { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public long? UserId { get; set; }
    }

    public class UpdateMunicipalityRequest
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(150)]
        public string? Department { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public long? UserId { get; set; }
    }
}

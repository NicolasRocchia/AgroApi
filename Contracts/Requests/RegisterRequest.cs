using System.ComponentModel.DataAnnotations;

namespace APIAgroConnect.Contracts.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido.")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT/CUIL es obligatorio.")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "El CUIT/CUIL debe tener 10 u 11 dígitos.")]
        public string TaxId { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }
    }
}

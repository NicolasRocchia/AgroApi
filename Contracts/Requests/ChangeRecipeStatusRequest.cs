using System.ComponentModel.DataAnnotations;

namespace APIAgroConnect.Contracts.Requests
{
    public class ChangeRecipeStatusRequest
    {
        [Required(ErrorMessage = "El estado es requerido.")]
        public string Status { get; set; } = string.Empty;
    }
}

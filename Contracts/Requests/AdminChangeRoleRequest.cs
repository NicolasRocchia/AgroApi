using System.ComponentModel.DataAnnotations;

namespace APIAgroConnect.Contracts.Requests
{
    public class AdminChangeRoleRequest
    {
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public long RoleId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models.DTO
{
    public class UserLoginRequestDTO
    {
        [Required]
        public string LoginId { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

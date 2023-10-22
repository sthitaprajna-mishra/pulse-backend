using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models.DTO
{
    public class TokenRequestDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

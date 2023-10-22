using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models.DTO
{
    public class UserRegistrationRequestDTO
    {
        public string DisplayPictureURL { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public DateTime DOB { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

using Microsoft.AspNetCore.Identity;

namespace CloseConnectv1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string BackgroundPictureURL { get; set; } = string.Empty;
        public string DisplayPictureURL { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public DateTime CreateDate { get; set; }
        public int NumberOfFriends { get; set; } 
    }
}

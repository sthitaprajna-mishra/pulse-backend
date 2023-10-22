using static CloseConnectv1.Utilities.Constants;

namespace CloseConnectv1.Models.DTO
{
    public class UserProfileDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public DateTime CreateDate { get; set; }
        public string DisplayPictureURL { get; set; } = string.Empty;
        public string BackgroundPictureURL { get; set; } = string.Empty;
        public SearchUserRelationshipCodes RelationshipWithLoggedInUser { get; set; }
    }
}

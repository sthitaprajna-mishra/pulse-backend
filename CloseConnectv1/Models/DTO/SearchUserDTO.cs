using static CloseConnectv1.Utilities.Constants;

namespace CloseConnectv1.Models.DTO
{
    public class SearchUserDTO : UserProfileDTO
    {
        public SearchUserRelationshipCodes RelationshipWithLoggedInUser { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}

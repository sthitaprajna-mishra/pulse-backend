using static CloseConnectv1.Utilities.Constants;

namespace CloseConnectv1.Models.DTO
{
    public class FriendDTO
    {
        public string FriendId { get; set; } = string.Empty; 
        public string FriendName { get; set; } = string.Empty;
        public string FriendUserName { get; set; } = string.Empty;
        public string FriendDPURL { get; set; } = string.Empty;
        public SearchUserRelationshipCodes RelationshipWithLoggedInUser { get; set; }
    }
}

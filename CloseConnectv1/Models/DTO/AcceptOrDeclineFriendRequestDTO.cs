namespace CloseConnectv1.Models.DTO
{
    public class AcceptOrDeclineFriendRequestDTO
    {
        public int FriendRequestId { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsDeclined { get; set; }
    }
}

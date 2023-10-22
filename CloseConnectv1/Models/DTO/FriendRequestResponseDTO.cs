namespace CloseConnectv1.Models.DTO
{
    public class FriendRequestResponseDTO
    {
        public int FriendRequestId { get; set; }
        public string SenderId { get; set; } = string.Empty; // User who sent the friend request
        public string SenderName { get; set; } = string.Empty;
        public string SenderUserName { get; set; } = string.Empty;
        public string SenderDPURL { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty; // User who received the friend request
        public bool IsAccepted { get; set; } // Indicates if the friend request is accepted
        public bool IsDeclined { get; set; } // Indicates if the friend request is declined
        public DateTime CreatedAt { get; set; }
        
    }
}

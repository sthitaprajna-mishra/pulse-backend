using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models
{
    public class FriendRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FriendRequestId { get; set; }
        public string SenderId { get; set; } = string.Empty; // User who sent the friend request
        public string ReceiverId { get; set; } = string.Empty; // User who received the friend request
        public bool IsAccepted { get; set; } // Indicates if the friend request is accepted
        public bool IsDeclined { get; set; } // Indicates if the friend request is declined
        public DateTime CreatedAt { get; set; }
    }
}

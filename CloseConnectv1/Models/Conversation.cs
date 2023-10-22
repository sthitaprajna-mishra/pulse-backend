using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models
{
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConversationId { get; set; }
        public string ParticipantOneId { get; set; } = String.Empty;
        public string ParticipantTwoId { get; set; } = String.Empty;
        public string ConversationPreview { get; set;} = String.Empty;
        public string ConversationPreviewUserId { get; set; } = String.Empty;   
        public bool IsRead { get; set; } = false;
        public DateTime LatestDate { get; set; }
    }
}

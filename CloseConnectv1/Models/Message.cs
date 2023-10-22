using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; } = String.Empty;
        public string ReceiverId { get; set; } = String.Empty;
        public string MessageText { get; set; } = String.Empty;
        public char IsDelete { get; set; } = 'N';
        public DateTime CreateDate { get; set; }
    }
}

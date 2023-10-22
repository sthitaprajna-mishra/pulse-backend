namespace CloseConnectv1.Models.DTO
{
    public class MessageDisplayDTO
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; } = String.Empty;
        public string ReceiverId { get; set; } = String.Empty;
        public string MessageText { get; set; } = String.Empty;
        public char IsDelete { get; set; } = 'N';
        public DateTime CreateDate { get; set; }
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantUserName { get; set; } = string.Empty;
        public string ParticipantDPURL { get; set; } = string.Empty;
    }
}

namespace CloseConnectv1.Models.DTO
{
    public class MessageRequestDTO
    {
        public int? ConversationId { get; set; }
        public char IsInitialMessage { get; set; }
        public string SenderId { get; set; } = String.Empty;
        public string ReceiverId { get; set; } = String.Empty;
        public string MessageText { get; set; } = String.Empty;
    }
}

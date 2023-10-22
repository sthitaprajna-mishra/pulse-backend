namespace CloseConnectv1.Models.DTO
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderUserName { get; set; } = string.Empty;
        public string SenderDPURL { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false; // Indicates if the notification has been read
        public string ActionUrl { get; set; } = string.Empty; // URL associated with the notification action
    }
}

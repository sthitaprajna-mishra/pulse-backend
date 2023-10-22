namespace CloseConnectv1.Models.DTO
{
    public class ConversationDisplayDTO
    {
        public int ConversationId { get; set; }
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantUserName { get; set; } = string.Empty;
        public string ParticipantDPURL { get; set; } = string.Empty;
        public string ConversationPreview { get; set; } = string.Empty;
        public string ConversationPreviewUserId { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime LatestDate { get; set; }
    }
}

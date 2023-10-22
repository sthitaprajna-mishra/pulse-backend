namespace CloseConnectv1.Models.DTO
{
    public class DraftCreateDTO
    {
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int CharacterCount { get; set; }
    }
}

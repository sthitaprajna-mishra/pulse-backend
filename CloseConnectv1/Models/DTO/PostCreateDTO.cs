namespace CloseConnectv1.Models.DTO
{
    public class PostCreateDTO
    {
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int CharacterCount { get; set; }
    }
}

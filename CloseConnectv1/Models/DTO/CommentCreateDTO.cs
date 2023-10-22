namespace CloseConnectv1.Models.DTO
{
    public class CommentCreateDTO
    {
        public string Text { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
    }
}

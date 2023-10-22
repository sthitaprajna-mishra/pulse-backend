namespace CloseConnectv1.Models.DTO
{
    public class CommentDisplayDTO
    {
        public int CommentId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUserName { get; set; } = string.Empty;  
        public string AuthorDisplayPictureURL { get; set; } = string.Empty;
        public bool IsLikedByCurrentUser { get; set; }
        public bool HasChildComments { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfComments { get; set; }
        public DateTime CreateDate { get; set; }
    }
}

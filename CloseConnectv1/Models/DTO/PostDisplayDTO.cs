using System.ComponentModel.DataAnnotations.Schema;

namespace CloseConnectv1.Models.DTO
{
    public class PostDisplayDTO
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUserName { get; set; } = string.Empty;
        public string AuthorDisplayPictureURL { get; set;} = string.Empty;
        public int CharacterCount { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfComments { get; set; }
        public int PopularityScore { get; set; }
    }
}

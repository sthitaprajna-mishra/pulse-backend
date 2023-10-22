using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloseConnectv1.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfComments { get; set; }
        public DateTime CreateDate{ get; set; }
    }
}

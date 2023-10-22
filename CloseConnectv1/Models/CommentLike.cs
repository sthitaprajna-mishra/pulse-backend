using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models
{
    public class CommentLike
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentLikeId { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CloseConnectv1.Models
{
    public class PostLike
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostLikeId { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}

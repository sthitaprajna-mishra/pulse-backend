using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloseConnectv1.Models
{
    public class Draft
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DraftId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public int CharacterCount { get; set; }
    }
}

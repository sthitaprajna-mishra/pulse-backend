namespace CloseConnectv1.Models
{
    public class EmailResult
    {
        public bool Sent { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

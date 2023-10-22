namespace CloseConnectv1.Models
{
    public class GenericResult
    {
        public object Result { get; set; } = new();
        public List<string> Errors { get; set; } = new List<string>();
    }
}

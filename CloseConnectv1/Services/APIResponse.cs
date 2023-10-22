using System.Net;

namespace CloseConnectv1.Services
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string Content { get; set; } = string.Empty;
    }
}

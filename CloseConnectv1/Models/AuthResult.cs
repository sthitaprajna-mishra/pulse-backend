using Microsoft.AspNetCore.Identity;
using System.Net;

namespace CloseConnectv1.Models
{
    public class AuthResult
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public bool Result { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> RoleIds { get; set; } = new List<string>();
        public int StatusCode { get; set; }
    }
}

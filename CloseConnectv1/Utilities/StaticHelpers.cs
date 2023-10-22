using CloseConnectv1.Models;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Text;

namespace CloseConnectv1.Utilities
{
    public class StaticHelpers
    {
        public static async Task<EmailResult> SendEmail(string body, string email)
        {
            try
            {
                using HttpClient client = new();

                client.BaseAddress = new Uri("https://sendemailserver.onrender.com/");

                var postData = new
                {
                    body,
                    receiverEmail = email
                };

                var content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync("sendemail", content);

                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (responseContent.ToString().Equals("email sent"))
                {
                    return new EmailResult
                    {
                        Sent = true
                    };
                }
            }
            catch(Exception ex)
            {
                return new EmailResult
                {
                    Sent = false,
                    Errors = new List<string>
                    {
                        "Email service error",
                        ex.Message
                    }
                };
            }

            return new EmailResult
            { Sent = false, Errors = new List<string> { "Email service error" } };
           
        }
        public static bool CheckIfEmail(string loginId)
        {
            var valid = true;

            try
            {
                var email = new MailAddress(loginId);
            }
            catch
            {
                valid = false;
            }

            return valid;
        }
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

            return dateTimeVal;
        }
        public static string RandomStringGeneration(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789abcdefghijklmnopqrstuvwxyz_";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static int? GetAncestorCommentId(List<Comment> comments, int commentId)
        {
            try
            {
                Comment? comment = comments.FirstOrDefault(c => c.CommentId == commentId);

                // If the comment is null or has no parent, it is the top-most parent comment
                if (comment is null || !comment.ParentCommentId.HasValue)
                {
                    return comment?.CommentId;
                }

                return GetAncestorCommentId(comments, comment.ParentCommentId.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}

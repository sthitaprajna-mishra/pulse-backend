using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CloseConnectv1.Filters
{
    public class TokenValidationFilter : IActionFilter
    {
        private readonly TokenValidator _tokenValidator;

        public TokenValidationFilter(TokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string token = context.HttpContext.Request.Headers["Authorization"].ToString();

            if (token.StartsWith("Bearer "))
            {
                token = token[7..];
                if (_tokenValidator.IsTokenExpired(token))
                {
                    context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                    return;
                }
            }
            else
            {
                context.Result = new BadRequestResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No additional action needed after the action is executed
        }
    }
}

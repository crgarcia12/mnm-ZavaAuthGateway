using System;
using System.Globalization;
using System.Web;

namespace ZavaAuthGateway
{
    public class ApiAuthValidateHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"valid\":false,\"error\":\"GET required\"}");
                return;
            }

            var token = (context.Request.QueryString["token"] ?? string.Empty).Trim();
            if (token.Length == 0)
            {
                token = (context.Request.Headers["X-Session-Token"] ?? string.Empty).Trim();
            }

            context.Response.ContentType = "application/json";
            if (token.Length == 0)
            {
                context.Response.StatusCode = 400;
                context.Response.Write("{\"valid\":false,\"error\":\"token is required\"}");
                return;
            }

            var repository = new AuthRepository();
            var session = repository.ValidateSessionToken(token);
            if (session == null)
            {
                context.Response.StatusCode = 200;
                context.Response.Write("{\"valid\":false}");
                return;
            }

            var userName = HttpUtility.JavaScriptStringEncode(session.Username ?? string.Empty);
            var expires = session.ExpiresAt.ToString("o", CultureInfo.InvariantCulture);
            context.Response.StatusCode = 200;
            context.Response.Write("{\"valid\":true,\"userId\":" + session.UserId + ",\"username\":\"" + userName + "\",\"expiresAt\":\"" + expires + "\"}");
        }
    }
}

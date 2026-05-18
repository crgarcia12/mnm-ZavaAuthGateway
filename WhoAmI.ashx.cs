using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

namespace ZavaAuthGateway
{
    public class WhoAmIHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");

            try
            {
                var cookie = context.Request.Cookies[".ZAVAAUTH"];
                if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                {
                    WriteNotAuthenticated(context);
                    return;
                }

                FormsAuthenticationTicket ticket;
                try
                {
                    ticket = FormsAuthentication.Decrypt(cookie.Value);
                }
                catch
                {
                    WriteNotAuthenticated(context);
                    return;
                }

                if (ticket == null || ticket.Expired)
                {
                    WriteNotAuthenticated(context);
                    return;
                }

                var repo = new AuthRepository();
                var user = repo.GetUserByUsername(ticket.Name);
                if (user == null || !user.IsActive || user.IsLockedOut)
                {
                    WriteNotAuthenticated(context);
                    return;
                }

                var roles = repo.GetUserRoles(user.UserId);
                var displayName = ((user.FirstName ?? string.Empty) + " " + (user.LastName ?? string.Empty)).Trim();
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = user.Username;
                }

                // The raw session token (GUID) is stored in FormsAuth ticket UserData.
                // Java apps can't decrypt the .ZAVAAUTH cookie, so the portal
                // passes this token via ?sessionToken= for cross-ecosystem SSO.
                var sessionToken = ticket.UserData ?? string.Empty;

                var sb = new StringBuilder();
                sb.Append("{\"authenticated\":true,\"username\":\"");
                sb.Append(HttpUtility.JavaScriptStringEncode(user.Username));
                sb.Append("\",\"displayName\":\"");
                sb.Append(HttpUtility.JavaScriptStringEncode(displayName));
                sb.Append("\",\"sessionToken\":\"");
                sb.Append(HttpUtility.JavaScriptStringEncode(sessionToken));
                sb.Append("\",\"roles\":[");
                for (int i = 0; i < roles.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append("\"");
                    sb.Append(HttpUtility.JavaScriptStringEncode(roles[i]));
                    sb.Append("\"");
                }
                sb.Append("]}");

                context.Response.Write(sb.ToString());
            }
            catch
            {
                WriteNotAuthenticated(context);
            }
        }

        private static void WriteNotAuthenticated(HttpContext context)
        {
            context.Response.Write("{\"authenticated\":false}");
        }
    }
}

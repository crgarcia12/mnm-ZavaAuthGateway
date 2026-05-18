using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace ZavaAuthGateway
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                try
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    if (ticket != null && !string.IsNullOrWhiteSpace(ticket.UserData))
                    {
                        var repository = new AuthRepository();
                        repository.DeactivateSessionToken(ticket.UserData);
                    }
                }
                catch
                {
                }
            }

            FormsAuthentication.SignOut();
            var expiredCookie = new HttpCookie(FormsAuthentication.FormsCookieName, string.Empty)
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = FormsAuthentication.FormsCookiePath,
                HttpOnly = true
            };
            Response.Cookies.Add(expiredCookie);

            Response.Redirect("Login.aspx?loggedOut=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}

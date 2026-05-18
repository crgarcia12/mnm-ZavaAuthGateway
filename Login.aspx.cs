using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace ZavaAuthGateway
{
    public partial class Login : Page
    {
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var username = (txtUsername.Text ?? string.Empty).Trim();
            var password = txtPassword.Text ?? string.Empty;

            if (username.Length == 0 || password.Length == 0)
            {
                lblError.Text = "Username and password are required.";
                return;
            }

            var repository = new AuthRepository();
            var user = repository.GetUserByUsername(username);
            if (user == null || !user.IsActive || user.IsLockedOut)
            {
                lblError.Text = "Invalid username or password.";
                return;
            }

            var computedHash = ComputeSha1Hex(user.Salt + password);
            if (!string.Equals(computedHash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                lblError.Text = "Invalid username or password.";
                return;
            }

            var sessionToken = Guid.NewGuid().ToString("N").ToUpperInvariant();
            repository.CreateSessionToken(user.UserId, sessionToken, Request.UserHostAddress, Request.UserAgent);
            repository.UpdateLastLogin(user.UserId);

            var now = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(
                2,
                user.Username,
                now,
                now.AddMinutes(30),
                false,
                sessionToken,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath,
                Expires = ticket.Expiration
            };

            Response.Cookies.Add(authCookie);
            Response.Redirect("Default.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static string ComputeSha1Hex(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha1.ComputeHash(bytes);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    builder.Append(b.ToString("X2"));
                }

                return builder.ToString();
            }
        }
    }
}

using System;
using System.Web.UI;

namespace ZavaAuthGateway
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            lblStatus.Text = "Authenticated user: " + User.Identity.Name;
        }
    }
}

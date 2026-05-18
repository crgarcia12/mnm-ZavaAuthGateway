<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ZavaAuthGateway.Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Zava Bank - Login</title>
</head>
<body>
    <div style="background:#b8c6d8;border-bottom:1px solid #8ea7c2;padding:4px 12px;font-family:Verdana,Arial;font-size:11px;"><a href="/" style="font-weight:bold;color:#1e4f8a;text-decoration:none;">&#9664; ZavaBank Portal</a></div>
    <h1>Zava Bank - Employee Login</h1>
    <form id="form1" runat="server">
        <div>
            <label for="txtUsername">Username:</label><br />
            <asp:TextBox ID="txtUsername" runat="server" /><br /><br />
            <label for="txtPassword">Password:</label><br />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" /><br /><br />
            <asp:Button ID="btnLogin" runat="server" Text="Sign In" OnClick="btnLogin_Click" />
            <br /><br />
            <asp:Label ID="lblError" runat="server" ForeColor="Red" />
        </div>
    </form>
</body>
</html>

using System;
using System.Data;
using System.Data.SqlClient;

namespace ZavaAuthGateway
{
    internal sealed class AuthRepository
    {
        public AuthUser GetUserByUsername(string username)
        {
            using (var connection = new SqlConnection(DbConfig.GetConnectionString()))
            using (var command = new SqlCommand(@"
SELECT UserID, Username, PasswordHash, Salt, IsActive, IsLockedOut, FirstName, LastName
FROM Users
WHERE Username = @Username;", connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new AuthUser
                    {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        PasswordHash = reader.GetString(2),
                        Salt = reader.GetString(3),
                        IsActive = reader.GetBoolean(4),
                        IsLockedOut = reader.GetBoolean(5),
                        FirstName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        LastName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                    };
                }
            }
        }

        public System.Collections.Generic.List<string> GetUserRoles(int userId)
        {
            var roles = new System.Collections.Generic.List<string>();
            using (var connection = new SqlConnection(DbConfig.GetConnectionString()))
            using (var command = new SqlCommand(@"
SELECT r.RoleName
FROM Roles r
INNER JOIN UserRoles ur ON r.RoleID = ur.RoleID
WHERE ur.UserID = @UserID;", connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(reader.GetString(0));
                    }
                }
            }
            return roles;
        }

        public void CreateSessionToken(int userId, string token, string ipAddress, string userAgent)
        {
            using (var connection = new SqlConnection(DbConfig.GetConnectionString()))
            using (var command = new SqlCommand(@"
INSERT INTO SessionTokens (UserID, Token, CreatedAt, ExpiresAt, SourceSystem, IsActive, IPAddress, UserAgent)
VALUES (@UserID, @Token, GETDATE(), DATEADD(MINUTE, 30, GETDATE()), @SourceSystem, 1, @IPAddress, @UserAgent);", connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@SourceSystem", "DotNet");
                command.Parameters.AddWithValue("@IPAddress", (object)(ipAddress ?? string.Empty));
                command.Parameters.AddWithValue("@UserAgent", (object)(userAgent ?? string.Empty));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeactivateSessionToken(string token)
        {
            using (var connection = new SqlConnection(DbConfig.GetConnectionString()))
            using (var command = new SqlCommand(@"
UPDATE SessionTokens
SET IsActive = 0, ExpiresAt = GETDATE()
WHERE Token = @Token;", connection))
            {
                command.Parameters.AddWithValue("@Token", token);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public SessionValidationResult ValidateSessionToken(string token)
        {
            using (var connection = new SqlConnection(DbConfig.GetConnectionString()))
            using (var command = new SqlCommand(@"
SELECT TOP 1 st.UserID, u.Username, st.ExpiresAt
FROM SessionTokens st
INNER JOIN Users u ON st.UserID = u.UserID
WHERE st.Token = @Token
  AND st.IsActive = 1
  AND st.ExpiresAt > GETDATE()
  AND u.IsActive = 1
ORDER BY st.ExpiresAt DESC;", connection))
            {
                command.Parameters.AddWithValue("@Token", token);
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new SessionValidationResult
                    {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        ExpiresAt = reader.GetDateTime(2)
                    };
                }
            }
        }

        public void UpdateLastLogin(int userId)
        {
            using (var connection = new SqlConnection(DbConfig.GetConnectionString()))
            using (var command = new SqlCommand("UPDATE Users SET LastLoginDate = GETDATE(), ModifiedDate = GETDATE() WHERE UserID = @UserID;", connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

    internal sealed class AuthUser
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        public bool IsActive { get; set; }

        public bool IsLockedOut { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    internal sealed class SessionValidationResult
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}

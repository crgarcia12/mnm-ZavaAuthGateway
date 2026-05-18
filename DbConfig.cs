using System;
using System.Configuration;

namespace ZavaAuthGateway
{
    internal static class DbConfig
    {
        public static string GetConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var database = Environment.GetEnvironmentVariable("DB_NAME");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(host) &&
                !string.IsNullOrWhiteSpace(port) &&
                !string.IsNullOrWhiteSpace(database) &&
                !string.IsNullOrWhiteSpace(user) &&
                !string.IsNullOrWhiteSpace(password))
            {
                return string.Format(
                    "Server={0},{1};Database={2};User Id={3};Password={4};TrustServerCertificate=true;",
                    host,
                    port,
                    database,
                    user,
                    password);
            }

            var configured = ConfigurationManager.ConnectionStrings["ZavaBankDb"];
            return configured != null ? configured.ConnectionString : string.Empty;
        }
    }
}

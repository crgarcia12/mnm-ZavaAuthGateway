# Architecture Diagram

ZavaAuthGateway is an ASP.NET Web Forms application (.NET Framework 4.8) that provides centralized authentication and session management for the ZavaBank platform, exposing both browser-based login flows and REST-style HTTP handler endpoints for token validation.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
        APIClient["API Client / Java Services"]
    end
    subgraph App["Application Layer - ASP.NET Web Forms (.NET Framework 4.8)"]
        LoginPage["Login.aspx (Login UI)"]
        LogoutPage["Logout.aspx (Logout UI)"]
        DefaultPage["Default.aspx (Landing Page)"]
        ApiValidate["ApiAuthValidate.ashx (Token Validation API)"]
        WhoAmI["WhoAmI.ashx (Identity API)"]
        Health["Health.ashx (Health Check)"]
        FormsAuth["ASP.NET Forms Authentication"]
    end
    subgraph DataAccess["Data Access Layer"]
        AuthRepo["AuthRepository (ADO.NET)"]
        DbConfig["DbConfig (Connection Factory)"]
    end
    subgraph Storage["Data Storage"]
        SQL[("SQL Server - ZavaBankDB")]
    end

    Browser -->|"HTTPS - login form"| LoginPage
    Browser -->|"HTTPS - logout"| LogoutPage
    Browser -->|"HTTPS - landing"| DefaultPage
    APIClient -->|"GET api/auth/validate?token="| ApiValidate
    APIClient -->|"GET WhoAmI.ashx"| WhoAmI
    APIClient -->|"GET health"| Health
    LoginPage -->|"authenticate"| FormsAuth
    LogoutPage -->|"sign out"| FormsAuth
    DefaultPage -->|"check identity"| FormsAuth
    WhoAmI -->|"decrypt cookie"| FormsAuth
    LoginPage -->|"lookup user"| AuthRepo
    ApiValidate -->|"validate session token"| AuthRepo
    WhoAmI -->|"lookup user and roles"| AuthRepo
    LogoutPage -->|"deactivate token"| AuthRepo
    AuthRepo -->|"build connection string"| DbConfig
    AuthRepo -->|"SQL queries"| SQL
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Presentation | ASP.NET Web Forms | .NET Framework 4.8 | Browser-based login/logout UI pages |
| API Endpoints | ASP.NET IHttpHandler | .NET Framework 4.8 | REST-style handlers for token validation and identity |
| Authentication | ASP.NET Forms Authentication | .NET Framework 4.8 | Cookie-based session management (.ZAVAAUTH cookie) |
| Data Access | ADO.NET (System.Data.SqlClient) | .NET Framework 4.8 | Direct SQL queries against SQL Server |
| Data Storage | SQL Server | 2019+ (Docker) | User accounts, roles, and session tokens |
| Runtime | .NET Framework | 4.8 | Application runtime |
| Container | Docker | - | Containerized deployment |

### Data Storage & External Services

The application relies on a single SQL Server database (`ZavaBankDB`) which stores user accounts (`Users`), role assignments (`Roles`, `UserRoles`), and active session tokens (`SessionTokens`). The connection string is resolved at runtime from environment variables (`DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`), with a fallback to the `web.config` `connectionStrings` section. No external caches, message brokers, or third-party API integrations are present.

### Key Architectural Decisions

- **Direct ADO.NET with raw SQL**: The application bypasses ORM frameworks and uses `SqlConnection`/`SqlCommand` directly, with parameterized queries for data access.
- **Forms Authentication for cross-ecosystem SSO**: The `.ZAVAAUTH` cookie carries an encrypted `FormsAuthenticationTicket` whose `UserData` field holds a raw GUID session token, allowing Java services (which cannot decrypt the cookie) to pass the token via query string to `api/auth/validate`.
- **SHA-1 salted password hashing**: Passwords are verified by computing `SHA1(salt + password)` and comparing against the stored `PasswordHash`.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation["Presentation"]
        LoginPage["Login.aspx"]
        LogoutPage["Logout.aspx"]
        DefaultPage["Default.aspx"]
    end
    subgraph Handlers["HTTP Handlers"]
        ApiValidate["ApiAuthValidateHandler"]
        WhoAmI["WhoAmIHandler"]
        Health["HealthHandler"]
    end
    subgraph Auth["Authentication Infrastructure"]
        FormsAuth["FormsAuthentication"]
    end
    subgraph DataAccess["Data Access"]
        AuthRepo["AuthRepository"]
        DbConfig["DbConfig"]
        AuthUser["AuthUser (Entity)"]
        SessionResult["SessionValidationResult (Entity)"]
    end

    LoginPage -->|"GetUserByUsername, CreateSessionToken, UpdateLastLogin"| AuthRepo
    LoginPage -->|"Encrypt + set cookie"| FormsAuth
    LogoutPage -->|"DeactivateSessionToken"| AuthRepo
    LogoutPage -->|"SignOut"| FormsAuth
    DefaultPage -->|"check IsAuthenticated"| FormsAuth
    ApiValidate -->|"ValidateSessionToken"| AuthRepo
    WhoAmI -->|"Decrypt cookie"| FormsAuth
    WhoAmI -->|"GetUserByUsername, GetUserRoles"| AuthRepo
    AuthRepo -->|"GetConnectionString"| DbConfig
    AuthRepo -->|"returns"| AuthUser
    AuthRepo -->|"returns"| SessionResult
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| Login.aspx / Login.aspx.cs | Presentation | Web Forms Page | Renders login form; validates credentials; creates session token; sets Forms Auth cookie |
| Logout.aspx / Logout.aspx.cs | Presentation | Web Forms Page | Deactivates session token in DB; clears Forms Auth cookie; redirects to login |
| Default.aspx / Default.aspx.cs | Presentation | Web Forms Page | Post-login landing page; redirects unauthenticated users to login |
| ApiAuthValidateHandler | HTTP Handlers | IHttpHandler | Validates a session token (via query string or header) against the database; returns JSON result |
| WhoAmIHandler | HTTP Handlers | IHttpHandler | Decrypts the .ZAVAAUTH cookie and returns user identity, display name, roles, and session token as JSON |
| HealthHandler | HTTP Handlers | IHttpHandler | Returns plain-text "OK" for liveness checks |
| AuthRepository | Data Access | Repository | Encapsulates all SQL Server interactions: user lookup, role fetch, session token CRUD, last-login update |
| DbConfig | Data Access | Configuration Utility | Builds the SQL Server connection string from environment variables or web.config fallback |
| AuthUser | Data Access | Entity / DTO | Represents a user record returned from the database |
| SessionValidationResult | Data Access | Entity / DTO | Represents a validated session token result with user identity and expiry |

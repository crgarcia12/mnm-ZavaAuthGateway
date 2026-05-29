# Architecture Diagram

This application is a single-service ASP.NET Web Forms authentication gateway. It uses forms authentication and SQL Server-backed repository calls to validate users and session tokens.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Browser or API Consumer"]
    end

    subgraph App["Application Layer - ASP.NET Web Forms (.NET Framework 4.8)"]
        Pages["Login Logout Default Pages"]
        Handlers["HTTP Handlers WhoAmI ApiAuthValidate Health"]
        Auth["Forms Authentication"]
        Repo["AuthRepository"]
    end

    subgraph Data["Data Layer"]
        SQL["ADO.NET SqlClient"]
        DB[("SQL Server AuthDb")]
    end

    Browser -->|"HTTP GET POST"| Pages
    Browser -->|"HTTP GET"| Handlers
    Pages -->|"issue and clear auth cookie"| Auth
    Handlers -->|"decrypt and validate auth"| Auth
    Pages -->|"query and update users"| Repo
    Handlers -->|"query users roles sessions"| Repo
    Repo -->|"SQL queries"| SQL
    SQL -->|"read and write"| DB
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Presentation | ASP.NET Web Forms / IHttpHandler | .NET Framework 4.8 | Serves login/logout pages and JSON handlers |
| Authentication | FormsAuthentication | .NET Framework 4.8 | Cookie-based authentication |
| Data Access | ADO.NET SqlClient | System.Data in framework | Executes parameterized SQL queries |
| Database | SQL Server | Configured in connection string | Stores users, roles, and session tokens |

### Data Storage & External Services

The application stores identity and session data in a SQL Server database (`AuthDb`) and does not call external service APIs.

### Key Architectural Decisions

- Uses direct repository-level SQL instead of an ORM.
- Uses cookie-based forms authentication for web and handler endpoints.
- Exposes dedicated lightweight handlers for health and token/session validation.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation
        LoginPage["Login.aspx.cs"]
        LogoutPage["Logout.aspx.cs"]
        DefaultPage["Default.aspx.cs"]
        WhoAmIHandler["WhoAmIHandler"]
        ValidateHandler["ApiAuthValidateHandler"]
        HealthHandler["HealthHandler"]
    end

    subgraph Business["Business Logic"]
        AuthRepo["AuthRepository"]
        AuthUser["AuthUser"]
        SessionResult["SessionValidationResult"]
    end

    subgraph DataAccess["Data Access"]
        DbConfig["DbConfig"]
        SqlClient["SqlConnection SqlCommand"]
    end

    LoginPage -->|"authenticates"| AuthRepo
    LogoutPage -->|"deactivates token"| AuthRepo
    WhoAmIHandler -->|"loads user and roles"| AuthRepo
    ValidateHandler -->|"validates token"| AuthRepo
    AuthRepo -->|"reads connection string"| DbConfig
    AuthRepo -->|"queries"| SqlClient
    AuthRepo -->|"password verify"| AuthUser
    ValidateHandler -->|"result handling"| SessionResult
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| Login.aspx.cs | Presentation | WebForms Page | Validates credentials and issues auth cookie |
| Logout.aspx.cs | Presentation | WebForms Page | Deactivates session token and signs out |
| WhoAmIHandler | Presentation | HTTP Handler | Returns authenticated user and roles |
| ApiAuthValidateHandler | Presentation | HTTP Handler | Validates session token and returns status |
| HealthHandler | Presentation | HTTP Handler | Returns health probe response |
| AuthRepository | Business Logic | Repository | Runs SQL for users, roles, and session tokens |
| DbConfig | Data Access | Configuration Helper | Resolves `AuthDb` connection string |

# Configuration & Externalized Settings Inventory

ZavaAuthGateway uses two configuration sources — `web.config` (static XML) and OS environment variables — with no runtime profile system, no secret store integration, and no feature flag framework.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| `web.config` | Static XML (ASP.NET) | `/web.config` | Primary config file; contains connection string, Forms Authentication settings, machine key, HTTP handler registrations, and authorization rules |
| Environment variables | OS/container env | Runtime (Docker `ENV` or host) | `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`; override the `web.config` connection string when all five are present |
| `ZavaAuthGateway.csproj` | MSBuild project file | `/ZavaAuthGateway.csproj` | Declares referenced assemblies and target framework (v4.8); no conditional build properties |
| `packages.config` | NuGet package manifest | `/packages.config` | Empty — no third-party packages |
| `Dockerfile` | Container build definition | `/Dockerfile` | Builds with Mono 6.12 / mcs; exposes port 8080; no build-arg or ENV declarations |

No Spring Cloud Config, Azure App Configuration, AWS AppConfig, Consul, HashiCorp Vault, or Azure KeyVault references detected.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default MSBuild configuration | Development build with debug symbols (`<compilation debug="true">`) | Standard .NET Framework BCL assemblies |
| Release | Manual (`/p:Configuration=Release`) | Production-optimized build | Same assembly set; no conditional NuGet packages |
| Docker (Mono) | `docker build` | Builds and runs on Mono 6.12 via `mcs` compiler | `mono:6.12` base image, `mono-xsp4` web server |

No Maven/Gradle build profiles. No webpack/vite configurations. No conditional compilation symbols defined in the project file.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default (all environments) | Single configuration — no profile switching | `web.config` | Connection string overridden via env vars at runtime |

No `appsettings.{Environment}.json`, no Spring profile-specific files, no `ASPNETCORE_ENVIRONMENT`, no `.env` variants. The application has a single runtime configuration with environment variables providing the only runtime override mechanism.

## Properties Inventory

### ZavaAuthGateway

| Property Key | Default (web.config) | Profile | Source |
|---|---|---|---|
| `connectionStrings["ZavaBankDb"]` | `Server=sqlserver,1433;Database=ZavaBankDB;User Id=sa;******;TrustServerCertificate=true;` | All | `web.config` |
| `DB_HOST` | _(not set)_ | All | Environment variable |
| `DB_PORT` | _(not set)_ | All | Environment variable |
| `DB_NAME` | _(not set)_ | All | Environment variable |
| `DB_USER` | _(not set)_ | All | Environment variable |
| `DB_PASSWORD` | _(not set)_ | All | Environment variable |
| `system.web/compilation[@debug]` | `true` | All | `web.config` |
| `system.web/compilation[@targetFramework]` | `4.8` | All | `web.config` |
| `system.web/httpRuntime[@targetFramework]` | `4.8` | All | `web.config` |
| `system.web/customErrors[@mode]` | `Off` | All | `web.config` |
| `system.web/authentication[@mode]` | `Forms` | All | `web.config` |
| `forms[@loginUrl]` | `~/Login.aspx` | All | `web.config` |
| `forms[@timeout]` | `30` (minutes) | All | `web.config` |
| `forms[@name]` | `.ZAVAAUTH` | All | `web.config` |
| `forms[@protection]` | `All` | All | `web.config` |
| `forms[@slidingExpiration]` | `true` | All | `web.config` |
| `forms[@path]` | `/` | All | `web.config` |
| `machineKey[@validationKey]` | `CB2721ABDAF8E9DC516D621D8B8BF13A2C9E8689A25303BF` | All | `web.config` — **hardcoded; see Secrets section** |
| `machineKey[@decryptionKey]` | `E9D2490BD0075B51D1BA5288514514AF` | All | `web.config` — **hardcoded; see Secrets section** |
| `machineKey[@validation]` | `SHA1` | All | `web.config` |
| `machineKey[@decryption]` | `AES` | All | `web.config` |
| `system.web/authorization` (default) | `<deny users="?">` | All | `web.config` |
| XSP4 port | `8080` | All | `CMD` in `Dockerfile` |
| XSP4 address | `0.0.0.0` | All | `CMD` in `Dockerfile` |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory Limit | Instance Count |
|---|---|---|---|
| ZavaAuthGateway | `xsp4 --port 8080 --address 0.0.0.0 --nonstop` (Mono 6.12) | Not specified (no Docker `--memory`, no K8s `resources.limits`) | Not specified |

No JVM heap settings (Java not used). No `ASPNETCORE_ENVIRONMENT`. No resource limits declared in `Dockerfile` or any orchestration file.

## Startup Dependency Chain

1. **SQL Server** must be reachable before the application can serve authenticated requests.
   - No `depends_on`, `dockerize`, Kubernetes readiness probe, or startup retry mechanism is configured.
   - If SQL Server is unavailable at startup, the application will start successfully but every data-access operation will fail with a `SqlException` at runtime.

No Docker Compose file is present. No Kubernetes manifests detected.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Current Storage |
|---|---|---|
| `connectionStrings["ZavaBankDb"]` → `****** | Database password | **Hardcoded plaintext in `web.config`** |
| `machineKey[@validationKey]` = `CB2721ABDAF8E9DC516D621D8B8BF13A2C9E8689A25303BF` | Forms Auth HMAC key | **Hardcoded plaintext in `web.config`** |
| `machineKey[@decryptionKey]` = `E9D2490BD0075B51D1BA5288514514AF` | Forms Auth AES encryption key | **Hardcoded plaintext in `web.config`** |
| `DB_PASSWORD` | Database password | Environment variable (not set by default; expected to be injected by container orchestrator or host) |

### Secrets Provisioning Workflow

There is no integrated secrets provisioning workflow. Secrets are currently handled in two ways:

1. **Hardcoded in `web.config`** (development/default): The SQL Server password and the Forms Authentication machine key are committed directly in `web.config`. These values are exposed in source control and must be rotated.
2. **Environment variables** (intended production path): `DbConfig` checks for all five `DB_*` environment variables at connection-string build time. If all are present, they supersede the `web.config` value. No framework (KeyVault, Vault, AWS Secrets Manager) bridges these env vars — they must be injected externally by the container runtime, CI/CD pipeline, or host OS.

The `machineKey` has no environment-variable override mechanism; it can only be changed by editing `web.config`. For production deployments it should be managed via a secrets store and injected at deploy time, not committed to the repository.

## Feature Flags

No feature flag framework is configured. No `@ConditionalOnProperty`, `IFeatureManager`, LaunchDarkly, Unleash, or custom toggle logic detected.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target Framework | .NET Framework 4.8 | `ZavaAuthGateway.csproj` `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` |
| ASP.NET Web Forms | 4.8 (bundled with .NET Fx) | `web.config` `targetFramework="4.8"` |
| ASP.NET Forms Authentication | 4.8 (bundled) | `web.config` `<authentication mode="Forms">` |
| ADO.NET / System.Data.SqlClient | 4.8 (bundled) | `ZavaAuthGateway.csproj` `<Reference Include="System.Data" />` |
| Mono | 6.12 | `Dockerfile` `FROM mono:6.12` |
| XSP4 (Mono web server) | Bundled with Mono 6.12 | `Dockerfile` `apt-get install -y mono-xsp4` |
| mcs (Mono C# compiler) | Bundled with Mono 6.12 | `Dockerfile` `RUN mcs ...` |
| Docker base image | `mono:6.12` (Debian Buster) | `Dockerfile` |
| NuGet packages | None | `packages.config` (empty) |

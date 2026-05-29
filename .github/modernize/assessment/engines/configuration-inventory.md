# Configuration & Externalized Settings Inventory

The project uses a small set of configuration sources centered on `web.config`, project build settings, and container runtime settings. Profiles and feature flags are minimal, with sensitive database credentials currently stored directly in configuration.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| web.config | .NET runtime config | `/web.config` | Contains connection string, forms auth, authorization, and handler mappings |
| ZavaAuthGateway.csproj | Build config | `/ZavaAuthGateway.csproj` | Debug/Release build configuration and target framework |
| packages.config | NuGet package list | `/packages.config` | Declares `Microsoft.AspNet.Identity.Core` |
| Dockerfile | Container runtime config | `/Dockerfile` | Defines mono+xsp runtime and exposed port 8080 |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `Configuration=Debug` | Developer build with symbols and no optimization | .NET Framework references from csproj |
| Release | `Configuration=Release` | Optimized release build | Same framework references |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default | IIS/ASP.NET runtime loads default config | `web.config` | Forms auth and handler routes |
| Container runtime | Docker/xsp execution | `Dockerfile` | Port `8080`, mono xsp host |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `connectionStrings:AuthDb` | SQL Server connection string | Default | `web.config` |
| `system.web/authentication@mode` | `Forms` | Default | `web.config` |
| `system.web/forms@name` | `.ZAVAAUTH` | Default | `web.config` |
| `system.web/forms@loginUrl` | `~/Login.aspx` | Default | `web.config` |
| `system.web/forms@timeout` | `480` | Default | `web.config` |
| `system.web/forms@slidingExpiration` | `true` | Default | `web.config` |
| `system.web/compilation@targetFramework` | `4.8` | Default | `web.config` |
| `system.webServer/handlers/*` | custom handler mappings | Default | `web.config` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| ZavaAuthGateway (xsp4) | `xsp4 --port 8080 --address 0.0.0.0 --nonstop` | Not specified | 1 (implicit) |

## Startup Dependency Chain

1. Container runtime starts mono xsp host.
2. ASP.NET loads `web.config` and handler/page mappings.
3. Application requests requiring data depend on SQL Server (`AuthDb`) availability.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `connectionStrings:AuthDb` password | Database credential | `web.config` value currently inline `[MASKED]` |

### Secrets Provisioning Workflow

Secrets are currently configured statically in `web.config` rather than through an external secret manager. No managed identity, vault integration, or automated secret injection workflow is present in repository configuration.

## Feature Flags

No feature flags or conditional configuration toggles were detected.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Framework target | 4.8 | `ZavaAuthGateway.csproj` |
| ASP.NET Web stack | System.Web on .NET Framework | framework references in `ZavaAuthGateway.csproj` |
| Microsoft.AspNet.Identity.Core | 2.2.1 | `packages.config` |
| Mono base image | 6.12 | `Dockerfile` |

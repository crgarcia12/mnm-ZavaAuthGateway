# Modernization Plan: Zava Auth Gateway modernization and Azure deployment

**Project**: ZavaAuthGateway

---

## Technical Framework

- **Language**: C# / .NET Framework 4.8 (ASP.NET Web Forms)
- **Framework**: ASP.NET Web Forms on .NET Framework 4.8
- **Build Tool**: MSBuild (.csproj)
- **Database**: SQL Server (ADO.NET via `System.Data.SqlClient`)
- **Key Dependencies**: System.Web, Forms Authentication, ADO.NET

---

## Overview

> This migration modernizes the Zava authentication gateway and deploys it to Azure.
> The application currently runs as a legacy ASP.NET Web Forms app on .NET Framework.
> The target state modernizes runtime and operating model with Azure-native services
> and cloud deployment:
>
> - Upgrade to the latest supported .NET framework target for long-term support
> - Adopt Azure-native service patterns and identity-based access where applicable
> - Deploy the modernized application to Azure in a cloud-native hosting model
>
> The migration follows a phased path: runtime upgrade, service modernization,
> security remediation, then deployment.

---

## Migration Impact Summary

| Application | Original Service | New Azure Service | Auth | Comments |
|-------------|------------------|-------------------|------|----------|
| ZavaAuthGateway | On-prem IIS/Web Forms | Azure Container Apps | Managed Identity | Modernize app and deploy to Azure |
| ZavaAuthGateway | SQL Server auth/session | Azure SQL Database | Managed Identity | Move to cloud-native data access |
| ZavaAuthGateway | Local app settings | Azure App Configuration | Managed Identity | Centralize non-secret config |
| ZavaAuthGateway | Local secrets/config | Azure Key Vault | Managed Identity | Externalize secrets securely |

---

## Open Questions & Questionnaire

- [x] Q: Include infrastructure provisioning? → A: No (not explicitly requested)
- [x] Q: Include integration testing task? → A: No (not explicitly requested)
- [x] Q: Include security/CVE remediation? → A: Yes (default and required)
- [x] Q: Azure deployment target? → A: Azure Container Apps (default)
- [x] Q: Include containerization task? → A: No separate task (covered by deployment)


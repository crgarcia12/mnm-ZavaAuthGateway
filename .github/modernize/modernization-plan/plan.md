# Modernization Plan: modernization-plan

**Project**: mnm-ZavaAuthGateway

---

## Technical Framework

- **Language**: C# / .NET Framework 4.8
- **Framework**: ASP.NET Web Forms (legacy .csproj format)
- **Build Tool**: MSBuild / dotnet build
- **Database**: Not explicitly defined in repository configuration
- **Key Dependencies**: System.Web, System.Configuration, System.Data

---

## Overview

This migration modernizes the authentication gateway for Azure readiness while
keeping current behavior stable.

The application currently runs as a legacy ASP.NET Web Forms project targeting
.NET Framework 4.8. The modernization effort will:

- improve dependency security posture before cloud rollout
- establish a repeatable dependency remediation baseline for future migration
- reduce known vulnerability risk prior to broader Azure migration activities

The migration follows an incremental approach that starts with security
hardening as a non-functional modernization baseline.

---

## Migration Impact Summary

| Application       | Original Service        | New Azure Service | Authentication    | Comments |
|------------------|-------------------------|-------------------|-------------------|----------|
| ZavaAuthGateway  | On-prem/IIS-hosted app  | TBD               | Managed Identity  | Initial plan focuses on security baseline. |

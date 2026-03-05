# Quickstart: IES Backend

**Feature**: `001-ies-backend-implementation` | **Date**: 2026-03-04

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 10.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| SQL Server | 2019+ or LocalDB | LocalDB ships with Visual Studio |
| Visual Studio 2022 / VS Code | Latest | VS Code needs C# Dev Kit extension |
| Git | 2.x+ | |

## 1. Clone & Checkout

```bash
git clone <repository-url>
cd Intelligent_Employment_System
git checkout 001-ies-backend-implementation
```

## 2. Restore NuGet Packages

```bash
dotnet restore Intelligent_Employment_System.slnx
```

## 3. Configure appsettings

Edit `IES.api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IES_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKey_AtLeast32Characters!",
    "Issuer": "IES.api",
    "Audience": "IES.client",
    "ExpiryInHours": 24
  },
  "AiService": {
    "BaseUrl": "http://localhost:8000"
  },
  "FileStorage": {
    "BasePath": "Uploads",
    "MaxFileSizeBytes": 10485760,
    "AllowedExtensions": [".pdf", ".docx"]
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "IES Platform"
  }
}
```

> **Note**: For production, use `dotnet user-secrets` for sensitive values (JWT secret, SMTP password).

## 4. Create Database & Apply Migrations

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration (from repo root)
dotnet ef migrations add InitialCreate --project Infrastructure/Persistence --startup-project IES.api

# Apply migration
dotnet ef database update --project Infrastructure/Persistence --startup-project IES.api
```

## 5. Run the API

```bash
cd IES.api
dotnet run
```

The API will start at:
- **HTTPS**: `https://localhost:7001`
- **HTTP**: `http://localhost:5001`

(Ports configured in `IES.api/Properties/launchSettings.json`)

## 6. Verify

Open your browser or use curl:

```bash
# Health check — should return OpenAPI doc
curl https://localhost:7001/openapi/v1.json

# Test auth endpoint (after implementation)
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"test@example.com","password":"Test@1234","gender":0,"userType":"Candidate"}'
```

## 7. Project Structure Quick Reference

| Project | Purpose | Key Contents |
|---------|---------|-------------|
| `Core/Domain` | Entities, enums, repo interfaces | Models/, Enums/, Contracts/, Exceptions/ |
| `Core/Services.Abstractions` | Service interfaces, DTOs | I*Service.cs, DTOs/ |
| `Core/Services` | Business logic | *Service.cs, Mapping/ |
| `Infrastructure/Persistence` | EF Core, repos, infrastructure services | Data/, Repositories/, Services/, Migrations/ |
| `Infrastructure/Presentation` | Controllers, SignalR hubs, middleware | Controllers/, Hubs/, Middleware/ |
| `Shared` | Cross-cutting utilities | Pagination/ |
| `IES.api` | Composition root | Program.cs (DI + middleware pipeline) |

## 8. Dependency Graph

```
IES.api ──→ Persistence ──→ Domain
   │             │              ↑
   │             └──→ Services.Abstractions
   │                       ↑
   └──→ Presentation ─────┘
              │
              └──→ Services.Abstractions

Shared ← referenced by all projects
```

## Common Tasks

| Task | Command |
|------|---------|
| Build solution | `dotnet build Intelligent_Employment_System.slnx` |
| Run tests | `dotnet test Intelligent_Employment_System.slnx` |
| Add migration | `dotnet ef migrations add <Name> --project Infrastructure/Persistence --startup-project IES.api` |
| Update DB | `dotnet ef database update --project Infrastructure/Persistence --startup-project IES.api` |
| Clean build | `dotnet clean Intelligent_Employment_System.slnx` |

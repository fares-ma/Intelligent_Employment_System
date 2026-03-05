# Implementation Plan: IES Backend

**Branch**: `001-ies-backend-implementation` | **Date**: 2026-03-04 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-ies-backend-implementation/spec.md`

## Summary

Build the complete backend for the Intelligent Employment System (IES) — an AI-powered recruitment platform. The backend implements authentication (JWT + Identity with TPH inheritance), candidate/recruiter/company management, job posting & search, AI-powered resume scoring via a Python microservice, assessments, interviews (AI voice + live WebRTC via SignalR signaling), email/real-time notifications, and dashboards. Architecture: Onion/Clean with ASP.NET Core 10, EF Core + SQL Server, Repository + UnitOfWork patterns.

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: ASP.NET Core 10, EF Core 10, ASP.NET Identity, JWT Bearer Auth, SignalR, CsvHelper, Microsoft.Extensions.Http.Resilience  
**Storage**: SQL Server (EF Core Code First, TPH inheritance)  
**Testing**: xUnit + Moq + Microsoft.AspNetCore.Mvc.Testing  
**Target Platform**: Windows/Linux server (Kestrel)  
**Project Type**: Web API (REST) + SignalR hubs  
**Performance Goals**: 200 concurrent users, <2s API response, <30s AI scoring, <10s CSV export for 500+ rows  
**Constraints**: Long-lived JWT (24h, no refresh), resume upload ≤10MB (PDF/DOCX only), strict sequential application pipeline  
**Scale/Scope**: Graduation project MVP — 34 functional requirements, 17 entities, ~53 API endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution file is an empty template — no custom rules defined. No gates to enforce.
**Pre-Phase 0**: ✅ PASS (no rules)
**Post-Phase 1**: ✅ PASS (no rules)

## Project Structure

### Documentation (this feature)

```text
specs/001-ies-backend-implementation/
├── plan.md              # This file
├── spec.md              # Feature specification (completed)
├── research.md          # Phase 0: Technical research findings
├── data-model.md        # Phase 1: Entity definitions and relationships
├── quickstart.md        # Phase 1: Developer setup guide
├── contracts/           # Phase 1: API endpoint contracts
│   ├── auth.md
│   ├── candidates.md
│   ├── companies.md
│   ├── jobs.md
│   ├── ai.md
│   ├── assessments.md
│   ├── interviews.md
│   └── notifications.md
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
Intelligent_Employment_System.slnx
│
├── Core/
│   ├── Domain/                          # Entities, Enums, Repository Interfaces
│   │   ├── Domain.csproj
│   │   ├── Models/                      # Entity classes
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── CandidateUser.cs
│   │   │   ├── Recruiter.cs
│   │   │   ├── Company.cs
│   │   │   ├── JobPost.cs
│   │   │   ├── Resume.cs
│   │   │   ├── JobApplication.cs
│   │   │   ├── Skill.cs
│   │   │   ├── Assessment.cs
│   │   │   ├── Question.cs
│   │   │   ├── Interview.cs
│   │   │   ├── SavedJob.cs
│   │   │   ├── CandidateSkill.cs
│   │   │   ├── JobPostSkill.cs
│   │   │   ├── ResumeSkill.cs
│   │   │   ├── CandidateAssessment.cs
│   │   │   └── Notification.cs
│   │   ├── Enums/                       # Domain enums
│   │   │   ├── Gender.cs
│   │   │   ├── UserRole.cs
│   │   │   ├── ApplicationStatus.cs
│   │   │   ├── InterviewType.cs
│   │   │   ├── InterviewStatus.cs
│   │   │   ├── JobType.cs
│   │   │   ├── WorkLocation.cs
│   │   │   ├── JobLevel.cs
│   │   │   ├── QuestionType.cs
│   │   │   ├── AssessmentType.cs
│   │   │   └── SkillCategory.cs
│   │   ├── Contracts/                   # Repository interfaces
│   │   │   ├── IRepositoryBase.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── ICandidateRepository.cs
│   │   │   ├── IRecruiterRepository.cs
│   │   │   ├── ICompanyRepository.cs
│   │   │   ├── IJobPostRepository.cs
│   │   │   ├── IResumeRepository.cs
│   │   │   ├── IJobApplicationRepository.cs
│   │   │   ├── ISkillRepository.cs
│   │   │   ├── IAssessmentRepository.cs
│   │   │   ├── IInterviewRepository.cs
│   │   │   ├── ISavedJobRepository.cs
│   │   │   └── INotificationRepository.cs
│   │   └── Exceptions/                  # Domain exceptions
│   │       ├── NotFoundException.cs
│   │       ├── BadRequestException.cs
│   │       └── ForbiddenException.cs
│   │
│   ├── Services/                        # Service implementations
│   │   ├── Services.csproj
│   │   ├── AuthService.cs
│   │   ├── CandidateService.cs
│   │   ├── CompanyService.cs
│   │   ├── JobPostService.cs
│   │   ├── JobApplicationService.cs
│   │   ├── AssessmentService.cs
│   │   ├── InterviewService.cs
│   │   ├── DashboardService.cs
│   │   ├── NotificationService.cs
│   │   └── Mapping/                     # DTO mapping profiles
│   │
│   └── Services.Abstractions/           # Service interfaces + DTOs
│       ├── Services.Abstractions.csproj
│       ├── IAuthService.cs
│       ├── ICandidateService.cs
│       ├── ICompanyService.cs
│       ├── IJobPostService.cs
│       ├── IJobApplicationService.cs
│       ├── IAssessmentService.cs
│       ├── IInterviewService.cs
│       ├── IDashboardService.cs
│       ├── IAiServiceClient.cs
│       ├── IEmailService.cs
│       ├── INotificationService.cs
│       ├── IFileStorageService.cs
│       └── DTOs/                        # Request/Response DTOs
│           ├── Auth/
│           ├── Candidates/
│           ├── Companies/
│           ├── Jobs/
│           ├── Applications/
│           ├── Assessments/
│           ├── Interviews/
│           ├── Notifications/
│           └── Dashboard/
│
├── Infrastructure/
│   ├── Persistence/                     # EF Core + Repositories
│   │   ├── Persistence.csproj
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Configurations/          # EF Core entity configs
│   │   │       ├── ApplicationUserConfig.cs
│   │   │       ├── CompanyConfig.cs
│   │   │       ├── JobPostConfig.cs
│   │   │       ├── ResumeConfig.cs
│   │   │       ├── JobApplicationConfig.cs
│   │   │       ├── SkillConfig.cs
│   │   │       ├── AssessmentConfig.cs
│   │   │       ├── InterviewConfig.cs
│   │   │       ├── SavedJobConfig.cs
│   │   │       └── NotificationConfig.cs
│   │   ├── Repositories/               # Repository implementations
│   │   │   ├── RepositoryBase.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── CandidateRepository.cs
│   │   │   ├── RecruiterRepository.cs
│   │   │   ├── CompanyRepository.cs
│   │   │   ├── JobPostRepository.cs
│   │   │   ├── ResumeRepository.cs
│   │   │   ├── JobApplicationRepository.cs
│   │   │   ├── SkillRepository.cs
│   │   │   ├── AssessmentRepository.cs
│   │   │   ├── InterviewRepository.cs
│   │   │   ├── SavedJobRepository.cs
│   │   │   └── NotificationRepository.cs
│   │   ├── Services/                   # Infrastructure services
│   │   │   ├── AiServiceClient.cs
│   │   │   ├── EmailService.cs
│   │   │   └── FileStorageService.cs
│   │   └── Migrations/                 # EF Core migrations
│   │
│   └── Presentation/                   # Controllers + Hubs
│       ├── Presentation.csproj
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CandidatesController.cs
│       │   ├── CompaniesController.cs
│       │   ├── JobsController.cs
│       │   ├── AiController.cs
│       │   ├── AssessmentsController.cs
│       │   ├── InterviewsController.cs
│       │   └── NotificationsController.cs
│       ├── Hubs/
│       │   ├── InterviewHub.cs
│       │   └── NotificationHub.cs
│       └── Middleware/
│           └── GlobalExceptionHandler.cs
│
├── Shared/                              # Cross-cutting concerns
│   ├── Shared.csproj
│   └── Pagination/
│       ├── PaginationParams.cs
│       └── PagedResult.cs
│
├── IES.api/                             # Composition root
│   ├── IES.api.csproj
│   ├── Program.cs                       # DI registration, middleware pipeline
│   ├── appsettings.json                 # Connection strings, JWT config, AI service URL
│   └── appsettings.Development.json
│
└── Tests/                               # Test projects (future)
    ├── IES.UnitTests/
    └── IES.IntegrationTests/
```

**Structure Decision**: The existing Onion Architecture project structure is retained as-is. All 7 projects already exist with correct dependency references. Controllers will live in `Infrastructure/Presentation` (not `IES.api/Controllers`). The API project is the composition root only — it wires DI and middleware. The existing `WeatherForecast` scaffold will be removed.

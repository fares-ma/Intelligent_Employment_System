# Tasks: IES Backend Implementation

**Input**: Design documents from `/specs/001-ies-backend-implementation/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Unit Tests for services (xUnit + Moq) + Integration Tests for Auth (WebApplicationFactory). Test tasks in Phase 12.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Domain entities/enums/contracts**: `Core/Domain/`
- **Service interfaces & DTOs**: `Core/Services.Abstractions/`
- **Service implementations**: `Core/Services/`
- **EF Core, repositories, infra services**: `Infrastructure/Persistence/`
- **Controllers, hubs, middleware**: `Infrastructure/Presentation/`
- **Cross-cutting utilities**: `Shared/`
- **Composition root**: `IES.api/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install NuGet packages, remove scaffold, establish project-level configurations

- [X] T001 Install NuGet packages per research.md: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer` in Infrastructure/Persistence/Persistence.csproj; `Microsoft.AspNetCore.Authentication.JwtBearer` and `Microsoft.EntityFrameworkCore.Tools` in IES.api/IES.api.csproj; `Microsoft.Extensions.Http.Resilience` in Infrastructure/Persistence/Persistence.csproj; `CsvHelper` in Infrastructure/Presentation/Presentation.csproj; `AutoMapper.Extensions.Microsoft.DependencyInjection` in Core/Services/Services.csproj
- [X] T002 Remove WeatherForecast scaffold: delete IES.api/WeatherForecast.cs and IES.api/Controllers/WeatherForecastController.cs
- [X] T003 [P] Create pagination utilities: PaginationParams class in Shared/Pagination/PaginationParams.cs and PagedResult<T> class in Shared/Pagination/PagedResult.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 [P] Create all domain enums in Core/Domain/Enums/: Gender.cs, UserRole.cs, ApplicationStatus.cs, InterviewType.cs, InterviewStatus.cs, JobLevel.cs, JobType.cs, WorkLocation.cs, QuestionType.cs, AssessmentType.cs, SkillCategory.cs (one file per enum, values per data-model.md)
- [X] T005 [P] Create domain exception classes: NotFoundException.cs, BadRequestException.cs, ForbiddenException.cs in Core/Domain/Exceptions/
- [X] T006 [P] Create ApplicationUser base entity (abstract, inherits IdentityUser) in Core/Domain/Models/ApplicationUser.cs with fields per data-model.md
- [X] T007 Create CandidateUser entity (inherits ApplicationUser) in Core/Domain/Models/CandidateUser.cs with navigation properties per data-model.md
- [X] T008 Create Recruiter entity (inherits ApplicationUser) in Core/Domain/Models/Recruiter.cs with CompanyId FK and navigation properties per data-model.md
- [X] T009 [P] Create Company entity in Core/Domain/Models/Company.cs with all fields, relationships, and validation rules per data-model.md
- [X] T010 [P] Create Skill entity in Core/Domain/Models/Skill.cs with Name (unique) and Category per data-model.md
- [X] T011 [P] Create junction entities: CandidateSkill.cs, JobPostSkill.cs, ResumeSkill.cs in Core/Domain/Models/ with composite keys per data-model.md
- [X] T012 [P] Create generic repository interface IRepositoryBase<T> in Core/Domain/Contracts/IRepositoryBase.cs with GetByIdAsync, GetAllAsync, Create, Update, Delete per research.md
- [X] T013 [P] Create IUnitOfWork interface in Core/Domain/Contracts/IUnitOfWork.cs exposing all repository properties and SaveChangesAsync
- [X] T014 Create AppDbContext (inherits IdentityDbContext<ApplicationUser>) in Infrastructure/Persistence/Data/AppDbContext.cs with DbSets for all entities and TPH discriminator configuration per research.md
- [X] T015 [P] Create EF entity configurations: ApplicationUserConfig.cs (TPH discriminator, indexes) and CompanyConfig.cs (TaxNumber unique index, field constraints) in Infrastructure/Persistence/Data/Configurations/
- [X] T016 [P] Create SkillConfig.cs (Name unique index) in Infrastructure/Persistence/Data/Configurations/SkillConfig.cs
- [X] T017 Create RepositoryBase<T> generic implementation in Infrastructure/Persistence/Repositories/RepositoryBase.cs with AsNoTracking for reads per research.md
- [X] T018 Create UnitOfWork implementation in Infrastructure/Persistence/Repositories/UnitOfWork.cs exposing all repositories and wrapping SaveChangesAsync
- [X] T019 [P] Create GlobalExceptionHandler middleware in Infrastructure/Presentation/Middleware/GlobalExceptionHandler.cs mapping NotFoundException→404, BadRequestException→400, ForbiddenException→403 with consistent JSON error envelope per research.md
- [X] T020 Configure Program.cs in IES.api/Program.cs: register EF Core with SQL Server connection string, ASP.NET Identity with ApplicationUser + IdentityRole, JWT Bearer authentication (24h expiry, ClockSkew=Zero, jti blacklist check in OnTokenValidated, SignalR token extraction in OnMessageReceived), add CORS, add SignalR, register UnitOfWork and repository DI, register service DI, add GlobalExceptionHandler middleware, map controllers and SignalR hubs per research.md and quickstart.md
- [X] T021 Configure appsettings.Development.json in IES.api/appsettings.Development.json with ConnectionStrings:DefaultConnection, JwtSettings (Secret, Issuer, Audience, ExpiryInHours), AiService:BaseUrl, FileStorage (BasePath, MaxFileSizeBytes, AllowedExtensions), EmailSettings per quickstart.md
- [X] T022 Create initial EF Core migration by running: dotnet ef migrations add InitialCreate --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 — User Registration & Authentication (Priority: P1) 🎯 MVP

**Goal**: Users can register as Candidate or Recruiter, log in to receive a JWT token with role, log out (token blacklisted), and reset passwords.

**Independent Test**: Create a candidate account, log in, verify token contains correct role, confirm token grants access to protected endpoints, log out and verify token is invalidated.

### Implementation for User Story 1

- [X] T023 [P] [US1] Create auth DTOs in Core/Services.Abstractions/DTOs/Auth/: RegisterRequestDto.cs (firstName, lastName, email, password, phoneNumber, gender, dateOfBirth, userType, companyId?), LoginRequestDto.cs (email, password), LoginResponseDto.cs (token, expiresAt, userId, email, role, userType), ForgotPasswordDto.cs (email), ResetPasswordDto.cs (email, token, newPassword), ChangePasswordDto.cs (currentPassword, newPassword)
- [X] T024 [P] [US1] Create IAuthService interface in Core/Services.Abstractions/IAuthService.cs with methods: RegisterAsync, LoginAsync, LogoutAsync, ForgotPasswordAsync, ResetPasswordAsync, ChangePasswordAsync per contracts/auth.md
- [X] T025 [US1] Implement AuthService in Core/Services/AuthService.cs: registration creates CandidateUser or Recruiter via UserManager, first recruiter for company gets Admin role, login generates JWT with role claims and unique jti, logout adds jti to IMemoryCache blacklist, forgot-password generates reset token (no info leak), reset-password validates token, change-password verifies current password per contracts/auth.md and research.md
- [X] T026 [US1] Create AuthController in Infrastructure/Presentation/Controllers/AuthController.cs with endpoints: POST /api/auth/register (201/400/409), POST /api/auth/login (200/401), POST /api/auth/logout [Authorize] (204), POST /api/auth/forgot-password (200), POST /api/auth/reset-password (200/400), POST /api/auth/change-password [Authorize] (200/400) per contracts/auth.md
- [X] T027 [US1] Create AutoMapper profile for Auth DTOs in Core/Services/Mapping/AuthMappingProfile.cs

**Checkpoint**: User Story 1 fully functional — registration, login, logout, password reset all working independently

---

## Phase 4: User Story 2 — Candidate Profile & Resume Management (Priority: P1)

**Goal**: Candidates update their profile (job title, summary, experience, skills), upload PDF/DOCX resumes (≤10MB), and request AI-generated CVs.

**Independent Test**: Create a candidate, update profile, add skills, upload a resume file, verify all data persisted and retrievable.

### Implementation for User Story 2

- [X] T028 [P] [US2] Create Resume entity in Core/Domain/Models/Resume.cs with all fields per data-model.md (OriginalFileName, StoredFilePath, FileType, FileSizeBytes, ExperienceSummary, EducationSummary, ActivitiesSummary, AiGeneratedCvPath, IsDefault)
- [X] T029 [P] [US2] Create SavedJob junction entity in Core/Domain/Models/SavedJob.cs with composite PK (CandidateId, JobPostId) per data-model.md
- [X] T030 [P] [US2] Create ICandidateRepository interface in Core/Domain/Contracts/ICandidateRepository.cs and IResumeRepository in Core/Domain/Contracts/IResumeRepository.cs and ISavedJobRepository in Core/Domain/Contracts/ISavedJobRepository.cs and ISkillRepository in Core/Domain/Contracts/ISkillRepository.cs
- [X] T031 [P] [US2] Create ResumeConfig.cs in Infrastructure/Persistence/Data/Configurations/ResumeConfig.cs and SavedJobConfig.cs in Infrastructure/Persistence/Data/Configurations/SavedJobConfig.cs (composite PK, indexes)
- [X] T032 [P] [US2] Create candidate DTOs in Core/Services.Abstractions/DTOs/Candidates/: CandidateProfileDto.cs, UpdateCandidateProfileDto.cs, UpdateSkillsDto.cs, ResumeDto.cs, SavedJobDto.cs, CandidateApplicationDto.cs
- [X] T033 [P] [US2] Create IFileStorageService interface in Core/Services.Abstractions/IFileStorageService.cs with SaveFileAsync, DeleteFileAsync, GetFileAsync methods
- [X] T034 [P] [US2] Create IAiServiceClient interface in Core/Services.Abstractions/IAiServiceClient.cs with ScoreResumeAsync, ExtractSkillsAsync, GenerateAssessmentAsync, GenerateCvAsync, GenerateInterviewQuestionsAsync, ScoreInterviewAsync methods per contracts/ai.md
- [X] T035 [US2] Implement FileStorageService in Core/Services/FileStorageService.cs: save to configured path, validate extension (.pdf/.docx) + magic bytes (PDF:%PDF, DOCX:PK..), generate random filename, enforce 10MB limit per research.md
- [X] T036 [US2] Implement CandidateRepository in Infrastructure/Persistence/Repositories/CandidateRepository.cs and ResumeRepository in Infrastructure/Persistence/Repositories/ResumeRepository.cs and SkillRepository in Infrastructure/Persistence/Repositories/SkillRepository.cs and SavedJobRepository in Infrastructure/Persistence/Repositories/SavedJobRepository.cs
- [X] T037 [P] [US2] Create ICandidateService interface in Core/Services.Abstractions/ICandidateService.cs with GetProfileAsync, UpdateProfileAsync, UpdateSkillsAsync, UploadResumeAsync, DeleteResumeAsync, GenerateCvAsync, GetApplicationsAsync, GetSavedJobsAsync, ToggleSaveJobAsync, UpdateProfilePictureAsync
- [X] T038 [US2] Implement CandidateService in Core/Services/CandidateService.cs: profile CRUD, skill replace, resume upload via IFileStorageService with validation, AI CV generation via IAiServiceClient, profile picture upload per contracts/candidates.md
- [X] T039 [US2] Create CandidatesController in Infrastructure/Presentation/Controllers/CandidatesController.cs with endpoints: GET /api/candidates/profile, PUT /api/candidates/profile, PUT /api/candidates/skills, POST /api/candidates/resume (multipart), DELETE /api/candidates/resume/{resumeId}, POST /api/candidates/resume/{resumeId}/generate-cv, PUT /api/candidates/profile-picture (multipart) — all [Authorize(Roles="Candidate")] per contracts/candidates.md
- [X] T040 [US2] Create AutoMapper profile for Candidate DTOs in Core/Services/Mapping/CandidateMappingProfile.cs
- [X] T041 [US2] Add EF migration for Resume and SavedJob entities: dotnet ef migrations add AddResumeAndSavedJob --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 2 fully functional — candidate profile management, skills, resume upload/delete, AI CV generation all working

---

## Phase 5: Auth Retrofit + Domain Updates (Design Review Changes)

**Purpose**: Apply all Session 2 design decisions to existing code: add new entities/enums, remove deprecated features, add 2 new registration endpoints, invite code system.

**⚠️ CRITICAL**: This phase modifies existing implemented code. Must be done before any new user story.

### 5A: Domain Model Updates

- [X] T042 [P] Add SkillLevel enum in Core/Domain/Enums/SkillLevel.cs (Beginner=1, Intermediate=2, Expert=3)
- [X] T043 [P] Remove Junior (value 2) from UserRole enum in Core/Domain/Enums/UserRole.cs — keep only Admin=0, Standard=1
- [X] T044 [P] Add Withdrawn=6 to ApplicationStatus enum in Core/Domain/Enums/ApplicationStatus.cs
- [X] T045 [P] Create CompanyInviteCode entity in Core/Domain/Models/CompanyInviteCode.cs (Id, CompanyId FK, Code unique 6-char, MaxUses default 5, CurrentUses default 0, ExpiresAt, CreatedByRecruiterId FK, IsActive, CreatedAt)
- [X] T046 [P] Create CandidateEducation entity in Core/Domain/Models/CandidateEducation.cs (Id, CandidateId FK, Degree, FieldOfStudy, Institution, GraduationYear int, CreatedAt)
- [X] T047 [P] Create CandidateExperience entity in Core/Domain/Models/CandidateExperience.cs (Id, CandidateId FK, JobTitle, Company, Description max 500, StartDate, EndDate?, CreatedAt)
- [X] T048 Add Level (SkillLevel enum, required, default Beginner) to CandidateSkill in Core/Domain/Models/CandidateSkill.cs
- [X] T049 Remove IsVerified from Company in Core/Domain/Models/Company.cs. Add nav prop: ICollection CompanyInviteCode
- [X] T050 Add nav props to CandidateUser.cs: ICollection CandidateEducation, ICollection CandidateExperience
- [X] T051 [P] Add DeletedAt (DateTime?) and DeletedBy (string?) to JobPost entity for soft delete
- [X] T052 [P] Add DeletedAt (DateTime?) and DeletedBy (string?) to JobApplication entity for soft delete

### 5B: Repository and EF Config Updates

- [X] T053 [P] Create ICompanyInviteCodeRepository in Core/Domain/Contracts/ with GetByCodeAsync, GetActiveByCompanyAsync
- [X] T054 [P] Create ICandidateEducationRepository in Core/Domain/Contracts/ with GetByCandidateAsync
- [X] T055 [P] Create ICandidateExperienceRepository in Core/Domain/Contracts/ with GetByCandidateAsync
- [X] T056 [P] Create ICompanyRepository and IRecruiterRepository in Core/Domain/Contracts/
- [X] T057 Add new repositories to IUnitOfWork interface
- [X] T058 [P] Create CompanyInviteCodeConfig.cs in Infrastructure/Persistence/Data/Configurations/ (Code unique index)
- [X] T059 [P] Create CandidateEducationConfig.cs and CandidateExperienceConfig.cs in Infrastructure/Persistence/Data/Configurations/
- [X] T060 Update CompanyConfig.cs: remove IsVerified column
- [X] T061 Update CandidateSkill config: add Level column with enum conversion
- [X] T062 Implement new repositories in Infrastructure/Persistence/Repositories/
- [X] T063 Register new repositories in UnitOfWork.cs

### 5C: Auth Retrofit (3 Registration Endpoints)

- [X] T064 [P] Create RegisterCompanyRequestDto.cs (personal 7 fields + Company Name, TaxNumber, Industry, Website?) and RegisterRecruiterRequestDto.cs (personal 7 fields + InviteCode)
- [X] T065 [P] Create IInviteCodeService interface in Core/Services.Abstractions/ with GenerateAsync, GetActiveCodesAsync, RevokeAsync, ValidateAndUseAsync
- [X] T066 Refactor AuthService: keep RegisterAsync for Candidate only. Add RegisterCompanyAsync (creates Company + Admin Recruiter). Add RegisterRecruiterAsync (validates invite code, creates Standard Recruiter)
- [X] T067 Implement InviteCodeService in Core/Services/InviteCodeService.cs
- [X] T068 Update AuthController: keep POST /api/auth/register (Candidate), add POST /api/auth/register/company, add POST /api/auth/register/recruiter
- [X] T069 Update AutoMapper for new auth DTOs

### 5D: Migration

- [X] T070 Add EF migration: DesignReviewUpdates (SkillLevel, Withdrawn, CompanyInviteCode, Education, Experience, remove IsVerified, soft delete fields)

**Checkpoint**: ✅ All design review changes applied. 3 registration endpoints. Invite code system. Domain model fully updated.

---

## Phase 6: User Story 3 — Company Management + Admin Transfer (Priority: P1)

**Goal**: Admin Recruiter edits company, generates invite codes, transfers admin role. Public company listing.

- [X] T071 [P] [US3] Create company DTOs: CompanyDto, CompanyDetailDto, UpdateCompanyDto, RecruiterDto, InviteCodeDto, GenerateInviteCodeDto, TransferAdminDto
- [X] T072 [P] [US3] Create ICompanyService interface with GetByIdAsync, GetAllAsync, UpdateAsync, UploadLogoAsync, GetRecruitersAsync, GetCompanyJobsAsync, TransferAdminAsync
- [X] T073 [US3] Implement CompanyService: update (admin only), logo upload, list recruiters, transfer admin (swap roles, same company), public info
- [X] T074 [US3] Create CompaniesController: GET /api/companies (public), GET /{id} (public), PUT /{id} [Admin], PUT /{id}/logo [Admin], GET /{id}/recruiters [Recruiter], GET /{id}/jobs (public), PUT /{id}/transfer-admin [Admin], POST /{id}/invite-codes [Admin], GET /{id}/invite-codes [Admin], DELETE /{id}/invite-codes/{codeId} [Admin]
- [X] T075 [US3] AutoMapper profile for Company DTOs

**Checkpoint**: US3 — company management, invite codes, admin transfer, public views all working (AutoMapper profile pending)

---

## Phase 7: User Story 2 Extension — Education, Experience, Skill Levels (Priority: P1)

**Goal**: Candidates add/edit/delete education and experience via CRUD endpoints. Skills include proficiency levels.

- [X] T076 [P] [US2] Create education DTOs: CreateEducationDto, UpdateEducationDto, EducationDto
- [X] T077 [P] [US2] Create experience DTOs: CreateExperienceDto, UpdateExperienceDto, ExperienceDto
- [X] T078 [P] [US2] Update UpdateSkillsDto: skills array with name + level (replace-all semantics)
- [X] T079 [US2] Update CandidateProfileDto to include education, experience, skill levels
- [X] T080 [US2] Add education/experience CRUD methods to ICandidateService
- [X] T081 [US2] Implement education/experience CRUD in CandidateService. Update UpdateSkillsAsync for SkillLevel
- [X] T082 [US2] Add education/experience endpoints to CandidatesController: POST/PUT/DELETE education and experience
- [X] T083 [US2] Update CandidateMappingProfile for education, experience, skill level mappings

**Checkpoint**: ✅ Candidate profile now includes education, experience, and skill proficiency levels

---

## Phase 8: User Story 4 — Job Posting and Management (Priority: P1)

**Goal**: Job CRUD with soft delete, RequiredLevel on skills, search with filters, similar jobs.

- [X] T084 [P] [US4] Create JobPost entity with all fields including DeletedAt/DeletedBy per data-model.md
- [X] T085 [P] [US4] Add RequiredLevel (SkillLevel?) to JobPostSkill junction entity
- [X] T086 [P] [US4] Create IJobPostRepository with GetPublishedJobsAsync, GetSimilarJobsAsync — exclude soft-deleted
- [X] T087 [P] [US4] Create JobPostConfig.cs with composite index (IsPublished, IsActive, ExpiryDate, DeletedAt), soft delete global query filter
- [X] T088 [P] [US4] Create job DTOs: JobListDto, JobDetailDto, CreateJobDto (skills with RequiredLevel), UpdateJobDto, PublishJobDto, JobFilterParams
- [X] T089 [P] [US4] Create IJobPostService with CreateAsync, SearchAsync, UpdateAsync, SoftDeleteAsync, PublishAsync, GetSimilarJobsAsync
- [X] T090 [US4] Implement JobPostRepository with soft delete filtering
- [X] T091 [US4] Implement JobPostService: create with RequiredLevel on skills, update (creator or admin, Standard edits own only), soft delete, search, similar jobs
- [X] T092 [US4] Create JobsController: POST, GET (public search), GET /{id}, PUT /{id}, DELETE /{id} (soft), PATCH /{id}/publish, GET /recruiter
- [X] T093 [US4] AutoMapper profile for Job DTOs
- [X] T094 [US4] Add EF migration: AddJobPost

**Checkpoint**: US4 — job CRUD with soft delete, search, similar jobs working (RequiredLevel on JobPostSkill + AutoMapper pending)

---

## Phase 9: User Story 5 — Job Application and Tracking (Priority: P2)

**Goal**: Apply for jobs (three-state MatchScore), withdraw while Pending, save/bookmark jobs.

- [X] T095 [P] [US5] Create JobApplication entity with DeletedAt/DeletedBy and three-state MatchScore
- [X] T096 [P] [US5] Create IJobApplicationRepository with duplicate check, candidate/job queries
- [X] T097 [P] [US5] Create JobApplicationConfig.cs with unique (CandidateId, JobPostId), soft delete filter
- [X] T098 [P] [US5] Create application DTOs: ApplyJobDto, CandidateApplicationDto, ApplicationDetailDto
- [X] T099 [P] [US5] Create IJobApplicationService with ApplyAsync, WithdrawAsync, GetCandidateApplicationsAsync
- [X] T100 [US5] Implement AiServiceClient with resilience pipeline (retry, circuit breaker, timeout)
- [X] T101 [US5] Register AiServiceClient in Program.cs
- [X] T102 [US5] Implement JobApplicationRepository
- [X] T103 [US5] Implement JobApplicationService: validate resume + published job + no duplicate (including Withdrawn), create with Pending + MatchScore=null, async AI scoring, WithdrawAsync (only Pending, terminal)
- [X] T104 [US5] Add endpoints: POST /api/jobs/{jobId}/apply [Candidate], PATCH withdraw [Candidate], GET/POST saved-jobs on CandidatesController
- [X] T105 [US5] Add EF migration: AddJobApplication

**Checkpoint**: ✅ US5 — applications with three-state scoring, withdraw, saved jobs all working

---

## Phase 10: User Story 6 — Applicant Management and Screening (Priority: P2)

**Goal**: Recruiter applicant management with pipeline, rating, CSV export.

- [X] T106 [P] [US6] Create applicant DTOs: ApplicantDto, ApplicantFilterParams, StatusChangeDto, RatingDto, ApplicantCsvExportDto
- [X] T107 [P] [US6] Add methods to IJobApplicationService: GetApplicantsAsync, UpdateStatusAsync, SetRatingAsync, ExportCsvAsync
- [X] T108 [US6] Implement pipeline validation: strict sequential + Withdrawn terminal state
- [X] T109 [US6] Implement CSV export using CsvHelper
- [X] T110 [US6] Add endpoints: GET applicants, PATCH status, PATCH rating, GET export — all on JobsController

**Checkpoint**: US6 — applicant listing, pipeline, rating, CSV export all working

---

## Phase 11: User Story 7 — AI Endpoints (Priority: P2)

**Goal**: AI proxy endpoints with graceful degradation.

- [ ] T111 [P] [US7] Create AI DTOs: ExtractSkills, AnalyzeResume, GenerateAssessment request/response
- [ ] T112 [US7] Create AiController: POST extract-skills, POST analyze-resume, POST generate-cv — 503 fallback
- [ ] T113 [US7] AutoMapper profile for AI DTOs

**Checkpoint**: US7 — AI proxy endpoints working with resilience

---

## Phase 12: User Story 8 — Assessments (Priority: P3)

**Goal**: Assessment CRUD with AI generation, candidate start/submit/score.

- [X] T114 [P] [US8] Create Assessment, Question, CandidateAssessment entities
- [X] T115 [P] [US8] Create IAssessmentRepository
- [X] T116 [P] [US8] Create AssessmentConfig.cs (unique CandidateId+AssessmentId)
- [X] T117 [P] [US8] Create assessment DTOs
- [X] T118 [P] [US8] Create IAssessmentService interface
- [X] T119 [US8] Implement AssessmentRepository
- [X] T120 [US8] Implement AssessmentService: create, AI generate, start, submit (time validation, auto-grade)
- [X] T121 [US8] Create AssessmentsController with all endpoints
- [X] T122 [US8] Add POST /api/ai/generate-assessment to AiController
- [X] T123 [US8] AutoMapper for Assessment DTOs
- [X] T124 [US8] Add EF migration: AddAssessments

**Checkpoint**: US8 — assessments fully functional

---

## Phase 13: User Story 9 — Interviews: Text-Based AI + Live (Priority: P3)

**Goal**: Text-based AI interviews (written Q&A, AI scores text) + live interviews via third-party WebRTC provider.

- [X] T125 [P] [US9] Create Interview entity with AiAnswers (not AiTranscript), MeetingLink for WebRTC
- [X] T126 [P] [US9] Create IInterviewRepository
- [X] T127 [P] [US9] Create InterviewConfig.cs
- [X] T128 [P] [US9] Create interview DTOs: CreateInterviewDto, InterviewDetailDto, AiInterviewQuestionsDto, SubmitAiInterviewDto (written answers), CompleteInterviewDto
- [X] T129 [P] [US9] Create IInterviewService with ScheduleAsync, GetAiQuestionsAsync, SubmitAiAnswersAsync, CreateLiveRoomAsync, CancelAsync
- [X] T130 [US9] Implement InterviewRepository
- [X] T131 [US9] Implement InterviewService: schedule (validate Interview stage), AI text interview (generate questions, candidate submits written answers, AI scores), live (create room via third-party, return link), cancel
- [X] T132 [US9] Create InterviewsController: POST, GET, GET list, POST ai-questions, POST submit-ai, POST join, PATCH complete, PATCH cancel
- [ ] T133 [US9] Add AI interview endpoints to AiController
- [ ] T134 [US9] AutoMapper for Interview DTOs
- [X] T135 [US9] Add EF migration: AddInterviews

**Checkpoint**: US9 — interviews partially working (AI controller endpoints + AutoMapper pending)

---

## Phase 14: User Story 10 — Notifications (Priority: P3)

**Goal**: Email (console dev, Mailtrap demo, SendGrid prod) + real-time in-app notifications via SignalR.

- [X] T136 [P] [US10] Create Notification entity
- [X] T137 [P] [US10] Create INotificationRepository
- [X] T138 [P] [US10] Create NotificationConfig.cs
- [ ] T139 [P] [US10] Create IEmailService interface
- [ ] T140 [P] [US10] Create notification DTOs: NotificationDto, UnreadCountDto
- [ ] T141 [P] [US10] Create INotificationService interface
- [X] T142 [US10] Implement NotificationRepository
- [ ] T143 [US10] Implement EmailService (console logging dev, Mailtrap demo, SendGrid prod)
- [ ] T144 [US10] Create NotificationHub (OnConnectedAsync, ReceiveNotification, UpdateUnreadCount)
- [ ] T145 [US10] Implement NotificationService: store + push via IHubContext + email
- [ ] T146 [US10] Integrate triggers into JobApplicationService, InterviewService, AssessmentService
- [ ] T147 [US10] Create NotificationsController: GET, GET unread-count, PATCH mark-read, PATCH read-all
- [ ] T148 [US10] Add EF migration: AddNotifications

**Checkpoint**: US10 — email + real-time notifications all working

---

## Phase 15: User Story 11 — Dashboards (Priority: P3)

**Goal**: Minimal dashboards — aggregate numbers only, no new complex endpoints.

- [ ] T149 [P] [US11] Create dashboard DTOs: CandidateDashboardDto, CompanyDashboardDto
- [ ] T150 [P] [US11] Create IDashboardService interface
- [ ] T151 [US11] Implement DashboardService: aggregate counts (no AI dependency)
- [ ] T152 [US11] Add GET /api/candidates/dashboard and GET /api/companies/dashboard
- [ ] T153 [US11] AutoMapper for Dashboard DTOs

**Checkpoint**: US11 — minimal dashboards returning aggregate data

---

## Phase 16: Tests + Polish

**Purpose**: Unit tests, integration tests, background services, final configuration.

### Tests

- [ ] T154 [P] Create IES.UnitTests project (xUnit + Moq)
- [ ] T155 [P] Create IES.IntegrationTests project (xUnit + WebApplicationFactory)
- [ ] T156 Unit tests: AuthService (register 3 paths, login, logout)
- [ ] T157 Unit tests: CandidateService (profile, skills with levels, education, experience)
- [ ] T158 Unit tests: JobApplicationService (apply, withdraw, pipeline, duplicates)
- [ ] T159 Unit tests: InviteCodeService (generate, validate, revoke, max uses, expiry)
- [ ] T160 Unit tests: CompanyService (update, transfer admin)
- [ ] T161 Integration tests: Auth endpoints via WebApplicationFactory

### Background Services

- [ ] T162 JobExpiryBackgroundService (IHostedService): every 24h, set IsActive=false for expired jobs
- [ ] T163 AiScoringRetryService (IHostedService): every 5min, retry AI scoring for MatchScore=null

### Polish

- [ ] T164 Configure Kestrel request size limits (11MB)
- [X] T165 CORS configuration for Angular frontend
- [ ] T166 Swagger/OpenAPI annotations on all controllers
- [X] T167 Verify complete DI registration in Program.cs
- [ ] T168 End-to-end flow: register candidate + company + invite recruiter + post job + apply + score + assess + text AI interview + accept + notifications
- [ ] T169 Run quickstart.md validation

---

## Dependencies and Execution Order

### Phase Dependencies

- **Phase 1-4**: ✅ Complete (Setup + Foundation + Auth + Candidate)
- **Phase 5 Auth Retrofit**: ✅ Complete (Domain updates + 3 registration endpoints + invite codes)
- **Phase 6 US3 Company**: ✅ Complete (AutoMapper profile pending)
- **Phase 7 US2 Extension**: ✅ Complete (Education, Experience, Skills with levels)
- **Phase 8 US4 Job**: ✅ Mostly Complete (RequiredLevel on JobPostSkill + AutoMapper pending)
- **Phase 9 US5 Application**: ✅ Complete (Apply, withdraw, saved jobs)
- **Phase 10 US6 Screening**: ❌ Not started
- **Phase 11 US7 AI**: ❌ Not started (AiServiceClient stub exists)
- **Phase 12 US8 Assessments**: ⚠️ Partially started (Entities + Config + Repo exist, Service/Controller/DTOs pending)
- **Phase 13 US9 Interviews**: ✅ Mostly Complete (AI controller endpoints + AutoMapper pending)
- **Phase 14 US10 Notifications**: ⚠️ Partially started (Entity + Config + Repo exist, Service/Controller/DTOs pending)
- **Phase 15 US11 Dashboards**: ❌ Not started
- **Phase 16 Tests + Polish**: ⚠️ Partially (CORS + DI done)

### Dependency Graph

```text
Phase 1-4: ✅ Complete
    |
Phase 5: ✅ Auth Retrofit + Domain Updates
    |
    +-- Phase 6: ✅ US3 Company (AutoMapper pending)
    |       |
    +-- Phase 7: ✅ US2 Extension
            |
        Phase 8: ✅ US4 Jobs (RequiredLevel + AutoMapper pending)
            |
        Phase 9: ✅ US5 Applications
            |
            +-- Phase 10: ❌ US6 Screening
            +-- Phase 11: ❌ US7 AI Endpoints
            +-- Phase 12: ⚠️ US8 Assessments (entities/repo done)
            +-- Phase 13: ✅ US9 Interviews (AI endpoints + AutoMapper pending)
            |       |
            +-- Phase 15: ❌ US11 Dashboards
            |
            Phase 14: ⚠️ US10 Notifications (entity/repo done)
                |
            Phase 16: ⚠️ Tests + Polish (CORS + DI done)
```

---

## Notes

- [P] tasks = different files, no dependencies — can run in parallel
- [Story] label maps task to specific user story
- Phases 1-9 (T001-T105) are ✅ COMPLETE (with minor gaps: T085, T075, T093)
- Phase 13 is ✅ mostly complete (T125-T132, T135 done)
- Phase 12 and 14 have foundational entities/repos done but need Services/Controllers/DTOs
- MVP = Phases 1-15 (11 user stories). Phase 16 = tests + polish
- Commit after each task or logical group
- Stop at any checkpoint to validate independently
- Total: ~169 tasks across 16 phases covering 11 user stories + tests
- **Updated**: 2026-03-26 — Tasks re-checked against actual codebase implementation
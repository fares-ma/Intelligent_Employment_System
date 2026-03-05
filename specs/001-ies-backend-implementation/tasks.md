# Tasks: IES Backend Implementation

**Input**: Design documents from `/specs/001-ies-backend-implementation/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Not explicitly requested in the feature specification. Test tasks are omitted.

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

- [ ] T028 [P] [US2] Create Resume entity in Core/Domain/Models/Resume.cs with all fields per data-model.md (OriginalFileName, StoredFilePath, FileType, FileSizeBytes, ExperienceSummary, EducationSummary, ActivitiesSummary, AiGeneratedCvPath, IsDefault)
- [ ] T029 [P] [US2] Create SavedJob junction entity in Core/Domain/Models/SavedJob.cs with composite PK (CandidateId, JobPostId) per data-model.md
- [ ] T030 [P] [US2] Create ICandidateRepository interface in Core/Domain/Contracts/ICandidateRepository.cs and IResumeRepository in Core/Domain/Contracts/IResumeRepository.cs and ISavedJobRepository in Core/Domain/Contracts/ISavedJobRepository.cs and ISkillRepository in Core/Domain/Contracts/ISkillRepository.cs
- [ ] T031 [P] [US2] Create ResumeConfig.cs in Infrastructure/Persistence/Data/Configurations/ResumeConfig.cs and SavedJobConfig.cs in Infrastructure/Persistence/Data/Configurations/SavedJobConfig.cs (composite PK, indexes)
- [ ] T032 [P] [US2] Create candidate DTOs in Core/Services.Abstractions/DTOs/Candidates/: CandidateProfileDto.cs, UpdateCandidateProfileDto.cs, UpdateSkillsDto.cs, ResumeDto.cs, SavedJobDto.cs, CandidateApplicationDto.cs
- [ ] T033 [P] [US2] Create IFileStorageService interface in Core/Services.Abstractions/IFileStorageService.cs with SaveFileAsync, DeleteFileAsync, GetFileAsync methods
- [ ] T034 [P] [US2] Create IAiServiceClient interface in Core/Services.Abstractions/IAiServiceClient.cs with ScoreResumeAsync, ExtractSkillsAsync, GenerateAssessmentAsync, GenerateCvAsync, GenerateInterviewQuestionsAsync, ScoreInterviewAsync methods per contracts/ai.md
- [ ] T035 [US2] Implement FileStorageService in Infrastructure/Persistence/Services/FileStorageService.cs: save to configured path, validate extension (.pdf/.docx) + magic bytes (PDF:%PDF, DOCX:PK..), generate random filename, enforce 10MB limit per research.md
- [ ] T036 [US2] Implement CandidateRepository in Infrastructure/Persistence/Repositories/CandidateRepository.cs and ResumeRepository in Infrastructure/Persistence/Repositories/ResumeRepository.cs and SkillRepository in Infrastructure/Persistence/Repositories/SkillRepository.cs and SavedJobRepository in Infrastructure/Persistence/Repositories/SavedJobRepository.cs
- [ ] T037 [P] [US2] Create ICandidateService interface in Core/Services.Abstractions/ICandidateService.cs with GetProfileAsync, UpdateProfileAsync, UpdateSkillsAsync, UploadResumeAsync, DeleteResumeAsync, GenerateCvAsync, GetApplicationsAsync, GetSavedJobsAsync, ToggleSaveJobAsync, UpdateProfilePictureAsync
- [ ] T038 [US2] Implement CandidateService in Core/Services/CandidateService.cs: profile CRUD, skill replace, resume upload via IFileStorageService with validation, AI CV generation via IAiServiceClient, profile picture upload per contracts/candidates.md
- [ ] T039 [US2] Create CandidatesController in Infrastructure/Presentation/Controllers/CandidatesController.cs with endpoints: GET /api/candidates/profile, PUT /api/candidates/profile, PUT /api/candidates/skills, POST /api/candidates/resume (multipart), DELETE /api/candidates/resume/{resumeId}, POST /api/candidates/resume/{resumeId}/generate-cv, PUT /api/candidates/profile-picture (multipart) — all [Authorize(Roles="Candidate")] per contracts/candidates.md
- [ ] T040 [US2] Create AutoMapper profile for Candidate DTOs in Core/Services/Mapping/CandidateMappingProfile.cs
- [ ] T041 [US2] Add EF migration for Resume and SavedJob entities: dotnet ef migrations add AddResumeAndSavedJob --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 2 fully functional — candidate profile management, skills, resume upload/delete, AI CV generation all working

---

## Phase 5: User Story 3 — Company Registration & Management (Priority: P1)

**Goal**: Super-admin creates companies (with unique tax number), admin recruiters edit company info and manage recruiters, public company listing.

**Independent Test**: Create a company, add a recruiter, edit company details, verify public company data accessible.

### Implementation for User Story 3

- [ ] T042 [P] [US3] Create ICompanyRepository interface in Core/Domain/Contracts/ICompanyRepository.cs and IRecruiterRepository interface in Core/Domain/Contracts/IRecruiterRepository.cs
- [ ] T043 [P] [US3] Create company DTOs in Core/Services.Abstractions/DTOs/Companies/: CompanyDto.cs, CompanyDetailDto.cs, CreateCompanyDto.cs, UpdateCompanyDto.cs, AddRecruiterDto.cs, RecruiterDto.cs, CompanyDashboardDto.cs
- [ ] T044 [P] [US3] Create ICompanyService interface in Core/Services.Abstractions/ICompanyService.cs with CreateAsync, GetByIdAsync, GetAllAsync, UpdateAsync, UploadLogoAsync, AddRecruiterAsync, GetRecruitersAsync, GetCompanyJobsAsync
- [ ] T045 [US3] Implement CompanyRepository in Infrastructure/Persistence/Repositories/CompanyRepository.cs and RecruiterRepository in Infrastructure/Persistence/Repositories/RecruiterRepository.cs
- [ ] T046 [US3] Implement CompanyService in Core/Services/CompanyService.cs: create company (unverified), enforce unique tax number (immutable), update company (admin recruiter of same company only), add recruiter (verify admin role), list recruiters per contracts/companies.md
- [ ] T047 [US3] Create CompaniesController in Infrastructure/Presentation/Controllers/CompaniesController.cs with endpoints: POST /api/companies [Admin], GET /api/companies (public, paginated), GET /api/companies/{id} (public), PUT /api/companies/{id} [AdminRecruiter], PUT /api/companies/{id}/logo [AdminRecruiter] (multipart), POST /api/companies/{id}/recruiters [AdminRecruiter], GET /api/companies/{id}/recruiters [Recruiter], GET /api/companies/{id}/jobs (public) per contracts/companies.md
- [ ] T048 [US3] Create AutoMapper profile for Company DTOs in Core/Services/Mapping/CompanyMappingProfile.cs

**Checkpoint**: User Story 3 fully functional — company management, recruiter assignment, public company views all working

---

## Phase 6: User Story 4 — Job Posting & Management (Priority: P1)

**Goal**: Recruiters create/edit/delete/publish jobs with skills, salary, and filters. Public job search with pagination. Similar jobs via rule-based matching.

**Independent Test**: Create a draft job, publish it, search with filters, verify it appears in listings with applicants count and similar jobs.

### Implementation for User Story 4

- [ ] T049 [P] [US4] Create JobPost entity in Core/Domain/Models/JobPost.cs with all fields per data-model.md (Title, Description, AiProcessedDescription, Location, JobType, WorkLocation, CareerLevel, SalaryMin/Max, Currency, ExpiryDate, IsPublished, IsActive, CompanyId FK, CreatedByRecruiterId FK)
- [ ] T050 [P] [US4] Create IJobPostRepository interface in Core/Domain/Contracts/IJobPostRepository.cs with GetPublishedJobsAsync(filters, pagination), GetByIdWithSkillsAsync, GetSimilarJobsAsync, GetCompanyJobsAsync
- [ ] T051 [P] [US4] Create JobPostConfig.cs in Infrastructure/Persistence/Data/Configurations/JobPostConfig.cs with composite index (IsPublished, IsActive, ExpiryDate), CompanyId index, field constraints, relationships per data-model.md
- [ ] T052 [P] [US4] Create job DTOs in Core/Services.Abstractions/DTOs/Jobs/: JobListDto.cs, JobDetailDto.cs, CreateJobDto.cs, UpdateJobDto.cs, PublishJobDto.cs, JobFilterParams.cs
- [ ] T053 [P] [US4] Create IJobPostService interface in Core/Services.Abstractions/IJobPostService.cs with CreateAsync, GetByIdAsync, SearchAsync, UpdateAsync, DeleteAsync, PublishAsync, GetRecruiterJobsAsync, GetSimilarJobsAsync
- [ ] T054 [US4] Implement JobPostRepository in Infrastructure/Persistence/Repositories/JobPostRepository.cs with published jobs filtering (IsPublished && IsActive && !expired), Include skills, similar jobs query (shared skills + same career level + same industry), paginated results
- [ ] T055 [US4] Implement JobPostService in Core/Services/JobPostService.cs: create job linked to recruiter's company, draft/publish toggle, update (creator or admin of same company), delete, search with all filters (location, jobType, workLocation, careerLevel, industry, salaryMin/Max), similar jobs via rule-based matching per contracts/jobs.md
- [ ] T056 [US4] Create JobsController in Infrastructure/Presentation/Controllers/JobsController.cs with endpoints: POST /api/jobs [Recruiter], GET /api/jobs (public, filtered, paginated), GET /api/jobs/{id} (public, includes applicantsCount + similarJobs), PUT /api/jobs/{id} [Recruiter], DELETE /api/jobs/{id} [Recruiter], PATCH /api/jobs/{id}/publish [Recruiter], GET /api/jobs/recruiter [Recruiter] per contracts/jobs.md
- [ ] T057 [US4] Create AutoMapper profile for Job DTOs in Core/Services/Mapping/JobMappingProfile.cs
- [ ] T058 [US4] Add EF migration for JobPost entity: dotnet ef migrations add AddJobPost --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 4 fully functional — job CRUD, publish/unpublish, search with filters, similar jobs all working. MVP complete (US1-US4).

---

## Phase 7: User Story 5 — Job Application & Tracking (Priority: P2)

**Goal**: Candidates apply for jobs (resume required, no duplicates), AI scoring triggered on submit, track application status, save/bookmark jobs.

**Independent Test**: Apply for a job, verify application recorded with Pending status, check AI scoring triggered, track status, toggle save job.

### Implementation for User Story 5

- [ ] T059 [P] [US5] Create JobApplication entity in Core/Domain/Models/JobApplication.cs with all fields per data-model.md (CandidateId, JobPostId, ResumeId FKs, Status, MatchScore, MatchReport, RecruiterRating, RecruiterNotes, AppliedAt)
- [ ] T060 [P] [US5] Create IJobApplicationRepository interface in Core/Domain/Contracts/IJobApplicationRepository.cs with GetByCandidateAsync(pagination), GetByJobPostAsync(filters, pagination), ExistsAsync(candidateId, jobPostId)
- [ ] T061 [P] [US5] Create JobApplicationConfig.cs in Infrastructure/Persistence/Data/Configurations/JobApplicationConfig.cs with unique index (CandidateId, JobPostId), status enum conversion, decimal precision for MatchScore, indexes per data-model.md
- [ ] T062 [P] [US5] Create application DTOs in Core/Services.Abstractions/DTOs/Applications/: ApplyJobDto.cs, CandidateApplicationDto.cs (if not already created), ApplicationDetailDto.cs
- [ ] T063 [P] [US5] Create IJobApplicationService interface in Core/Services.Abstractions/IJobApplicationService.cs with ApplyAsync, GetCandidateApplicationsAsync, GetByIdAsync
- [ ] T064 [US5] Implement AiServiceClient in Infrastructure/Persistence/Services/AiServiceClient.cs: typed HttpClient with IHttpClientFactory, resilience pipeline (retry 3x exponential, circuit breaker 50%/30s, timeout 45s), methods: ScoreResumeAsync, ExtractSkillsAsync, GenerateCvAsync, GenerateAssessmentAsync, GenerateInterviewQuestionsAsync, ScoreInterviewAsync per research.md and contracts/ai.md
- [ ] T065 [US5] Register AiServiceClient in IES.api/Program.cs: AddHttpClient<IAiServiceClient, AiServiceClient> with base URL from config, add standard resilience handler
- [ ] T066 [US5] Implement JobApplicationRepository in Infrastructure/Persistence/Repositories/JobApplicationRepository.cs with duplicate check, candidate applications query, job applicants query with sorting/filtering
- [ ] T067 [US5] Implement JobApplicationService in Core/Services/JobApplicationService.cs: validate resume exists, validate job published, prevent duplicates, create application with Pending status, trigger AI scoring (store null if unavailable per FR-034), get candidate applications with pagination per contracts/jobs.md
- [ ] T068 [US5] Add apply and saved-jobs endpoints to JobsController: POST /api/jobs/{jobId}/apply [Candidate] (201/400/404/409) per contracts/jobs.md. Add saved-jobs endpoints to CandidatesController: GET /api/candidates/saved-jobs, POST /api/candidates/saved-jobs/{jobPostId} per contracts/candidates.md
- [ ] T069 [US5] Add EF migration for JobApplication entity: dotnet ef migrations add AddJobApplication --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 5 fully functional — applications with AI scoring, tracking, saved jobs all working

---

## Phase 8: User Story 6 — Applicant Management & Screening (Priority: P2)

**Goal**: Recruiters view applicants sorted by score, filter by status, rate 1-5 stars, change pipeline status (strict sequential), export CSV.

**Independent Test**: View applicants for a job, set ratings, filter by status, move through pipeline, export to CSV.

### Implementation for User Story 6

- [ ] T070 [P] [US6] Create applicant DTOs in Core/Services.Abstractions/DTOs/Applications/: ApplicantDto.cs (applicationId, candidate summary, status, matchScore, recruiterRating, appliedAt), ApplicantFilterParams.cs, StatusChangeDto.cs, RatingDto.cs, ApplicantCsvExportDto.cs (with CsvHelper [Name] attributes)
- [ ] T071 [P] [US6] Add methods to IJobApplicationService: GetApplicantsAsync, UpdateStatusAsync (with pipeline validation), SetRatingAsync, ExportApplicantsCsvAsync
- [ ] T072 [US6] Implement pipeline status validation in JobApplicationService: enforce strict sequential transitions per data-model.md state machine (Pending→UnderReview→Assessment→Interview→Accepted/Rejected, reject from any stage, no skip, no backwards, terminal states)
- [ ] T073 [US6] Implement CSV export in JobApplicationService using CsvHelper: query applicants, map to ApplicantCsvExportDto, write to MemoryStream with InvariantCulture, return as byte array per research.md
- [ ] T074 [US6] Add applicant management endpoints to JobsController: GET /api/jobs/{jobId}/applicants [Recruiter] (paginated, filtered, sorted), PATCH /api/jobs/{jobId}/applicants/{applicationId}/status [Recruiter], PATCH /api/jobs/{jobId}/applicants/{applicationId}/rating [Recruiter], GET /api/jobs/{jobId}/applicants/export [Recruiter] (returns CSV file) per contracts/jobs.md

**Checkpoint**: User Story 6 fully functional — applicant list, filtering, rating, pipeline management, CSV export all working

---

## Phase 9: User Story 7 — AI Resume Scoring & Skill Extraction (Priority: P2)

**Goal**: AI service integration endpoints: resume analysis with match score, skill extraction from job descriptions, graceful degradation when AI unavailable.

**Independent Test**: Submit resume + job to AI analyze endpoint, verify score returned. Submit job description to skill extraction, verify structured skills returned.

### Implementation for User Story 7

- [ ] T075 [P] [US7] Create AI DTOs in Core/Services.Abstractions/DTOs/Ai/: ExtractSkillsRequestDto.cs, ExtractSkillsResponseDto.cs, AnalyzeResumeRequestDto.cs, AnalyzeResumeResponseDto.cs, GenerateAssessmentRequestDto.cs, GenerateAssessmentResponseDto.cs
- [ ] T076 [US7] Create AiController in Infrastructure/Presentation/Controllers/AiController.cs with endpoints: POST /api/ai/extract-skills [Recruiter], POST /api/ai/analyze-resume [Recruiter], POST /api/ai/generate-cv [Candidate] — all with 503 fallback when AI unavailable per contracts/ai.md
- [ ] T077 [US7] Add AutoMapper profile for AI DTOs in Core/Services/Mapping/AiMappingProfile.cs

**Checkpoint**: User Story 7 fully functional — AI proxy endpoints working with resilience fallback

---

## Phase 10: User Story 8 — Assessment Creation & Evaluation (Priority: P3)

**Goal**: Recruiters create assessments (manual or AI-generated) with MCQ/TrueFalse/OpenEnded/Coding questions. Candidates start, submit within time window, and receive scores.

**Independent Test**: Create an assessment with questions, have candidate start it, submit answers within time, verify scoring.

### Implementation for User Story 8

- [ ] T078 [P] [US8] Create Assessment entity in Core/Domain/Models/Assessment.cs and Question entity in Core/Domain/Models/Question.cs and CandidateAssessment entity in Core/Domain/Models/CandidateAssessment.cs with all fields per data-model.md
- [ ] T079 [P] [US8] Create IAssessmentRepository interface in Core/Domain/Contracts/IAssessmentRepository.cs with GetByJobPostAsync, GetWithQuestionsAsync
- [ ] T080 [P] [US8] Create AssessmentConfig.cs and related configs in Infrastructure/Persistence/Data/Configurations/AssessmentConfig.cs (Assessment, Question relationships, CandidateAssessment unique index on CandidateId+AssessmentId)
- [ ] T081 [P] [US8] Create assessment DTOs in Core/Services.Abstractions/DTOs/Assessments/: CreateAssessmentDto.cs, AssessmentDetailDto.cs, QuestionDto.cs, StartAssessmentResponseDto.cs, SubmitAnswersDto.cs, AssessmentResultDto.cs, CandidateAssessmentResultDto.cs
- [ ] T082 [P] [US8] Create IAssessmentService interface in Core/Services.Abstractions/IAssessmentService.cs with CreateAsync, GetByIdAsync, UpdateAsync, DeleteAsync, GetByJobPostAsync, StartAsync, SubmitAsync, GetResultsAsync
- [ ] T083 [US8] Implement AssessmentRepository in Infrastructure/Persistence/Repositories/AssessmentRepository.cs with Include(Questions), candidate assessment queries
- [ ] T084 [US8] Implement AssessmentService in Core/Services/AssessmentService.cs: create assessment with questions (auto-calculate totalScore), AI-generated assessment via IAiServiceClient, start assessment (record StartedAt, calculate deadlineAt), submit answers (reject if past deadline, auto-grade MCQ/TrueFalse, mark OpenEnded/Coding for manual review), hide correctAnswer from candidate responses per contracts/assessments.md
- [ ] T085 [US8] Create AssessmentsController in Infrastructure/Presentation/Controllers/AssessmentsController.cs with endpoints: POST /api/assessments [Recruiter], GET /api/assessments/{id}, PUT /api/assessments/{id} [Recruiter], DELETE /api/assessments/{id} [Recruiter], GET /api/assessments/job/{jobPostId} [Recruiter], POST /api/assessments/{id}/start [Candidate], POST /api/assessments/{id}/submit [Candidate], GET /api/assessments/{id}/results [Recruiter] per contracts/assessments.md
- [ ] T086 [US8] Create AutoMapper profile for Assessment DTOs in Core/Services/Mapping/AssessmentMappingProfile.cs
- [ ] T087 [US8] Add AI generate-assessment endpoint to AiController: POST /api/ai/generate-assessment [Recruiter] per contracts/ai.md
- [ ] T088 [US8] Add EF migration for Assessment, Question, CandidateAssessment entities: dotnet ef migrations add AddAssessments --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 8 fully functional — assessment CRUD, AI generation, candidate start/submit/score all working

---

## Phase 11: User Story 9 — Interview Scheduling & Conduct (Priority: P3)

**Goal**: Recruiters schedule AI voice and live video interviews. AI interviews generate questions, conduct via voice, score responses. Live interviews use SignalR + WebRTC signaling.

**Independent Test**: Schedule both interview types, start AI interview and verify score/transcript, join live interview room and verify signaling works.

### Implementation for User Story 9

- [ ] T089 [P] [US9] Create Interview entity in Core/Domain/Models/Interview.cs with all fields per data-model.md (JobApplicationId FK, InterviewType, Status, ScheduledAt, DurationMinutes, MeetingLink, AiQuestions, AiTranscript, Score, FeedbackNotes, CompletedAt)
- [ ] T090 [P] [US9] Create IInterviewRepository interface in Core/Domain/Contracts/IInterviewRepository.cs with GetByApplicationAsync, GetUpcomingAsync, GetByUserAsync(pagination)
- [ ] T091 [P] [US9] Create InterviewConfig.cs in Infrastructure/Persistence/Data/Configurations/InterviewConfig.cs with enum conversions, decimal precision for Score, JobApplication relationship
- [ ] T092 [P] [US9] Create interview DTOs in Core/Services.Abstractions/DTOs/Interviews/: CreateInterviewDto.cs, InterviewDetailDto.cs, InterviewListDto.cs, StartAiInterviewResponseDto.cs, CompleteAiInterviewDto.cs, CompleteInterviewDto.cs, CancelInterviewDto.cs
- [ ] T093 [P] [US9] Create IInterviewService interface in Core/Services.Abstractions/IInterviewService.cs with ScheduleAsync, GetByIdAsync, ListAsync, StartAiInterviewAsync, CompleteAiInterviewAsync, JoinLiveAsync, CompleteLiveAsync, CancelAsync
- [ ] T094 [US9] Implement InterviewRepository in Infrastructure/Persistence/Repositories/InterviewRepository.cs
- [ ] T095 [US9] Implement InterviewService in Core/Services/InterviewService.cs: schedule interview (validate application at Interview stage, generate meetingLink for live), start AI interview (validate time window 15min before, generate questions via IAiServiceClient), complete AI interview (score via IAiServiceClient, save transcript), join live (validate time window), complete live (save score + feedback), cancel (only if Scheduled) per contracts/interviews.md
- [ ] T096 [US9] Create InterviewHub in Infrastructure/Presentation/Hubs/InterviewHub.cs with SignalR methods: JoinRoom, LeaveRoom, SendOffer, SendAnswer, SendIceCandidate and server→client: UserJoined, UserLeft, ReceiveOffer, ReceiveAnswer, ReceiveIceCandidate — using Groups scoped to interviewId per contracts/interviews.md
- [ ] T097 [US9] Create InterviewsController in Infrastructure/Presentation/Controllers/InterviewsController.cs with endpoints: POST /api/interviews [Recruiter], GET /api/interviews/{id}, GET /api/interviews (paginated, filtered), POST /api/interviews/{id}/start-ai [Candidate], POST /api/interviews/{id}/complete-ai [Candidate], POST /api/interviews/{id}/join, PATCH /api/interviews/{id}/complete [Recruiter], PATCH /api/interviews/{id}/cancel [Recruiter] per contracts/interviews.md
- [ ] T098 [US9] Add AI interview-questions and score-interview endpoints to AiController: POST /api/ai/interview-questions [Recruiter], POST /api/ai/score-interview per contracts/ai.md
- [ ] T099 [US9] Create AutoMapper profile for Interview DTOs in Core/Services/Mapping/InterviewMappingProfile.cs
- [ ] T100 [US9] Add EF migration for Interview entity: dotnet ef migrations add AddInterview --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 9 fully functional — interview scheduling, AI voice interview, live WebRTC signaling all working

---

## Phase 12: User Story 10 — Notifications & Candidate Acceptance (Priority: P3)

**Goal**: Email notifications for key pipeline events (reminders, status changes, acceptance/rejection). Real-time in-app notifications via SignalR NotificationHub.

**Independent Test**: Trigger notification events, verify emails sent, verify in-app notifications delivered to connected users.

### Implementation for User Story 10

- [ ] T101 [P] [US10] Create Notification entity in Core/Domain/Models/Notification.cs with all fields per data-model.md (Id, UserId FK, Type, Title, Message, TargetUrl, IsRead, CreatedAt)
- [ ] T101b [P] [US10] Create INotificationRepository interface in Core/Domain/Contracts/INotificationRepository.cs with GetByUserAsync(userId, pagination, isRead?), GetUnreadCountAsync(userId), MarkReadAsync(id), MarkAllReadAsync(userId)
- [ ] T101c [P] [US10] Create NotificationConfig.cs in Infrastructure/Persistence/Data/Configurations/NotificationConfig.cs with UserId FK to ApplicationUser, index on (UserId, IsRead), field constraints per data-model.md
- [ ] T102 [P] [US10] Create IEmailService interface in Core/Services.Abstractions/IEmailService.cs with SendAsync(to, subject, body), SendInterviewReminderAsync, SendStatusChangeAsync, SendAcceptanceAsync
- [ ] T103 [P] [US10] Create notification DTOs in Core/Services.Abstractions/DTOs/Notifications/: NotificationDto.cs (id, type, title, message, targetUrl, isRead, createdAt), UnreadCountDto.cs
- [ ] T104 [P] [US10] Create INotificationService interface in Core/Services.Abstractions/INotificationService.cs with GetAsync(userId, pagination, isRead?), GetUnreadCountAsync, MarkReadAsync, MarkAllReadAsync, CreateAndPushAsync(userId, type, title, message)
- [ ] T104b [US10] Implement NotificationRepository in Infrastructure/Persistence/Repositories/NotificationRepository.cs with filtered queries, mark-read updates; register in UnitOfWork
- [ ] T105 [US10] Implement EmailService in Infrastructure/Persistence/Services/EmailService.cs using SmtpClient with settings from appsettings per research.md
- [ ] T106 [US10] Create NotificationHub in Infrastructure/Presentation/Hubs/NotificationHub.cs with OnConnectedAsync (join user group), server→client: ReceiveNotification, UpdateUnreadCount per contracts/notifications.md
- [ ] T107 [US10] Implement NotificationService in Core/Services/NotificationService.cs: store notification via INotificationRepository + IUnitOfWork, push via IHubContext<NotificationHub> to user group, send email for key events per contracts/notifications.md notification trigger matrix
- [ ] T108 [US10] Integrate notification triggers into existing services: JobApplicationService (application submitted → recruiter, status changed → candidate, accepted/rejected → candidate), InterviewService (interview scheduled → candidate+recruiter), AssessmentService (assessment stage → candidate)
- [ ] T109 [US10] Create NotificationsController in Infrastructure/Presentation/Controllers/NotificationsController.cs with endpoints: GET /api/notifications [Authorize] (paginated, filterable by isRead), GET /api/notifications/unread-count [Authorize], PATCH /api/notifications/{id}/read [Authorize], PATCH /api/notifications/read-all [Authorize] per contracts/notifications.md
- [ ] T110 [US10] Add EF migration for Notification entity: dotnet ef migrations add AddNotification --project Infrastructure/Persistence --startup-project IES.api

**Checkpoint**: User Story 10 fully functional — email notifications, real-time in-app notifications, acceptance/rejection flow all working

---

## Phase 13: User Story 11 — Dashboards & Analytics (Priority: P3)

**Goal**: Candidate dashboard (applications count, saved jobs, upcoming interviews, recent activity, AI-suggested jobs). Company dashboard (total applicants, active jobs, hire rate, trends, action items).

**Independent Test**: Call dashboard endpoints, verify aggregated metrics match underlying data.

### Implementation for User Story 11

- [ ] T111 [P] [US11] Create dashboard DTOs in Core/Services.Abstractions/DTOs/Dashboard/: CandidateDashboardDto.cs (applicationsCount, savedJobsCount, upcomingInterviews, recentActivity, aiSuggestedJobs), CompanyDashboardDto.cs (totalApplicants, activeJobs, hireRate, applicationTrends, recentJobPosts, actionRequired)
- [ ] T112 [P] [US11] Create IDashboardService interface in Core/Services.Abstractions/IDashboardService.cs with GetCandidateDashboardAsync, GetCompanyDashboardAsync
- [ ] T113 [US11] Implement DashboardService in Core/Services/DashboardService.cs: candidate dashboard aggregates applications count, saved jobs count, upcoming interviews, recent activity, AI-suggested jobs (via IAiServiceClient, gracefully omit if unavailable); company dashboard aggregates totalApplicants, activeJobs, hireRate (accepted/total), applicationTrends (weekly counts), recentJobPosts, actionRequired items per contracts/candidates.md and contracts/companies.md
- [ ] T114 [US11] Add GET /api/candidates/dashboard [Candidate] endpoint to CandidatesController per contracts/candidates.md. Add GET /api/companies/dashboard [Recruiter] endpoint to CompaniesController per contracts/companies.md
- [ ] T115 [US11] Create AutoMapper profile for Dashboard DTOs in Core/Services/Mapping/DashboardMappingProfile.cs

**Checkpoint**: User Story 11 fully functional — both dashboards returning accurate aggregated data

---

## Phase 14: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T115b [US5] Create AiScoringRetryService as BackgroundService in Infrastructure/Persistence/Services/AiScoringRetryService.cs: periodic IHostedService that queries JobApplications with MatchScore = null, retries AI scoring via IAiServiceClient, updates score on success, logs and skips on continued failure. Run every 5 minutes. Register in Program.cs with AddHostedService<AiScoringRetryService>() per FR-034
- [ ] T116 [P] Configure Kestrel request size limits in IES.api/Program.cs: MaxRequestBodySize=11_000_000, FormOptions.MultipartBodyLengthLimit=11_000_000 for file uploads per research.md
- [ ] T117 [P] Add CORS configuration in IES.api/Program.cs for Angular frontend (allow localhost:4200 in development) with appropriate headers and methods
- [ ] T118 [P] Add Swagger/OpenAPI documentation annotations to all controllers using XML comments and ProducesResponseType attributes
- [ ] T119 Register all remaining DI services in IES.api/Program.cs: verify all services, repositories, IUnitOfWork, IAiServiceClient, IFileStorageService, IEmailService, INotificationService, AutoMapper profiles are registered
- [ ] T120 Verify strict pipeline enforcement end-to-end: test full hiring flow (register → apply → score → assess → interview → accept) per SC-012
- [ ] T121 Run quickstart.md validation: follow all steps in quickstart.md to verify fresh setup works from clone to running API

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — **BLOCKS all user stories**
- **US1 Auth (Phase 3)**: Depends on Foundational — **BLOCKS US2-US11** (all stories need auth)
- **US2 Candidate (Phase 4)**: Depends on US1 (needs authenticated candidate)
- **US3 Company (Phase 5)**: Depends on US1 (needs authenticated recruiter)
- **US4 Job (Phase 6)**: Depends on US3 (jobs belong to companies) and US2 (skills shared)
- **US5 Application (Phase 7)**: Depends on US2 (resume required) and US4 (jobs to apply to)
- **US6 Screening (Phase 8)**: Depends on US5 (applications to manage)
- **US7 AI (Phase 9)**: Depends on US5 (AI client already built for scoring in US5)
- **US8 Assessment (Phase 10)**: Depends on US5 (application pipeline)
- **US9 Interview (Phase 11)**: Depends on US5 (application pipeline)
- **US10 Notifications (Phase 12)**: Depends on US5, US8, US9 (triggers from pipeline events)
- **US11 Dashboards (Phase 13)**: Depends on US2, US4, US5 (aggregates from all entities)
- **Polish (Phase 14)**: Depends on all desired user stories being complete

### User Story Dependencies

```text
Phase 1: Setup
    ↓
Phase 2: Foundational
    ↓
Phase 3: US1 (Auth) ← BLOCKS ALL
    ↓
    ├── Phase 4: US2 (Candidate)
    │       ↓
    └── Phase 5: US3 (Company)
            ↓
        Phase 6: US4 (Job) ← depends on US2 + US3
            ↓
        Phase 7: US5 (Application) ← depends on US2 + US4
            ↓
            ├── Phase 8: US6 (Screening)
            ├── Phase 9: US7 (AI Endpoints)
            ├── Phase 10: US8 (Assessments)
            ├── Phase 11: US9 (Interviews)
            │       ↓
            └── Phase 12: US10 (Notifications) ← depends on US5+US8+US9
                    ↓
                Phase 13: US11 (Dashboards)
                    ↓
                Phase 14: Polish
```

### Within Each User Story

- DTOs and interfaces (marked [P]) can be created in parallel
- Entity configs can parallel with DTOs
- Repository implementations before service implementations
- Service implementations before controller implementations
- Controllers last (depend on services)
- Migrations after all entities for that story are defined

### Parallel Opportunities

- **Phase 2**: T004, T005, T006, T009, T010, T011, T012, T013, T015, T016, T019 can all run in parallel
- **Phase 4 (US2)**: T028-T034 (entities, DTOs, interfaces) can all run in parallel
- **Phase 5 (US3)**: T042-T044 can run in parallel
- **Phase 6 (US4)**: T049-T053 can run in parallel
- **Phase 7 (US5)**: T059-T063 can run in parallel
- **Phase 8 (US6)**: T070-T071 can run in parallel
- **Phase 10 (US8)**: T078-T082 can run in parallel
- **Phase 11 (US9)**: T089-T093 can run in parallel
- **Phase 12 (US10)**: T101-T104 can run in parallel
- **Phase 14**: T116-T118 can run in parallel

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Batch 1 — All parallel (different files, no dependencies):
Task T004: Create all domain enums
Task T005: Create domain exceptions
Task T006: Create ApplicationUser base entity
Task T009: Create Company entity
Task T010: Create Skill entity
Task T011: Create junction entities
Task T012: Create IRepositoryBase<T>
Task T013: Create IUnitOfWork
Task T015: Create EF configs (ApplicationUser, Company)
Task T016: Create SkillConfig
Task T019: Create GlobalExceptionHandler

# Batch 2 — Depends on T006:
Task T007: Create CandidateUser (inherits ApplicationUser)
Task T008: Create Recruiter (inherits ApplicationUser)

# Batch 3 — Depends on all entities + interfaces:
Task T014: Create AppDbContext
Task T017: Create RepositoryBase<T>
Task T018: Create UnitOfWork

# Batch 4 — Depends on everything above:
Task T020: Configure Program.cs
Task T021: Configure appsettings

# Batch 5 — Depends on DbContext:
Task T022: Create initial migration
```

---

## Implementation Strategy

### MVP First (User Stories 1–4)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 — Auth
4. Complete Phase 4: US2 — Candidate Profiles + Phase 5: US3 — Companies (can parallel)
5. Complete Phase 6: US4 — Jobs
6. **STOP and VALIDATE**: Test full flow: register → login → create company → post job → search → verify
7. Deploy/demo if ready — this is your core MVP

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Auth) → Test independently → First working endpoint
3. Add US2 (Candidate) + US3 (Company) → Test independently → Profile + Company management
4. Add US4 (Jobs) → Test independently → **MVP Complete!**
5. Add US5 (Applications) → Test independently → Applications + AI scoring
6. Add US6 (Screening) + US7 (AI) → Test independently → Full recruiter workflow
7. Add US8 (Assessments) + US9 (Interviews) → Test independently → Evaluation pipeline
8. Add US10 (Notifications) + US11 (Dashboards) → Test independently → Polish layer
9. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- No test tasks generated (not requested in spec) — add tests separately if needed
- Total: 125 tasks across 14 phases covering 11 user stories

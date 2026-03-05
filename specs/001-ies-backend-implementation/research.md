# Research: IES Backend Implementation

**Feature**: `001-ies-backend-implementation` | **Date**: 2026-03-04

## 1. ASP.NET Identity with TPH Inheritance

**Decision**: Use `IdentityUser` as base `ApplicationUser`, derive `CandidateUser` and `Recruiter` using TPH (EF Core's default inheritance strategy).

**Rationale**: TPH stores all types in a single table with a discriminator column. Microsoft guidance confirms TPH is the recommended default for most applications. It avoids JOINs for polymorphic queries and integrates cleanly with ASP.NET Identity's `UserManager`.

**Alternatives Considered**:
- TPT (Table-Per-Type): Separate tables per type with JOINs. Rejected — slower queries, more complexity, no benefit for this use case.
- TPC (Table-Per-Concrete-Type): Separate tables with duplicated columns. Rejected — no shared table for Identity operations, breaks `UserManager`.

**Key Implementation Notes**:
- Register Identity with base type: `AddIdentity<ApplicationUser, IdentityRole>()`
- Configure discriminator explicitly: `HasDiscriminator<string>("UserType").HasValue<CandidateUser>("Candidate").HasValue<Recruiter>("Recruiter")`
- All subtype columns are nullable in the database (CandidateUser.JobTitle is NULL for Recruiter rows)
- Query subtypes via `DbContext.Set<CandidateUser>()` or `Users.OfType<CandidateUser>()` — EF auto-adds WHERE clause
- Never register separate `UserManager<CandidateUser>` — use one `UserManager<ApplicationUser>` and cast

**NuGet**: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

## 2. Repository + UnitOfWork Pattern

**Decision**: Hybrid approach — generic `RepositoryBase<T>` for CRUD + specific repository interfaces for domain queries. `IUnitOfWork` owns all repositories and `SaveChangesAsync`.

**Rationale**: Generic repos reduce boilerplate. Specific repos encapsulate complex queries (eager loading, filtering) without leaking `IQueryable`. UnitOfWork ensures transactional consistency across multiple repository operations.

**Alternatives Considered**:
- Pure generic repository: Rejected — complex queries (e.g., get published jobs with skills, filtered and paginated) don't fit a generic interface.
- No repository (direct DbContext in services): Rejected — violates Onion Architecture dependency rules; services would depend on infrastructure.
- CQRS/MediatR: Rejected — overengineered for a graduation project with straightforward CRUD + some queries.

**Key Implementation Notes**:
- `IRepositoryBase<T>` in Domain/Contracts: `GetByIdAsync`, `GetAllAsync`, `Create`, `Update`, `Delete`
- Never return `IQueryable<T>` from repositories — return `IEnumerable<T>`, `Task<T?>`, or `Task<PagedResult<T>>`
- Specific repos add methods like `GetPublishedJobsAsync(filters, pagination)` with `.Include()` baked in
- Use `AsNoTracking()` for read-only queries
- `SaveChanges()` only in UnitOfWork, never in individual repositories

## 3. JWT Authentication (Long-Lived, No Refresh)

**Decision**: JWT Bearer with 24-hour expiry, role claims embedded, in-memory blacklist for logout invalidation.

**Rationale**: Simple approach suitable for a graduation project. Long-lived token avoids the complexity of refresh token rotation. In-memory blacklist via `IMemoryCache` handles logout token invalidation for the 200-user scale.

**Alternatives Considered**:
- Short-lived + refresh token: Rejected per clarification — adds complexity without proportional benefit for this project.
- Session cookies: Rejected — not suitable for SPA frontend (Angular) or mobile clients.
- Token revocation via database: Rejected — IMemoryCache is sufficient for 200 users and avoids DB hits on every request.

**Key Implementation Notes**:
- Set `ClockSkew = TimeSpan.Zero` to prevent tokens living 5min past expiry
- Generate unique `jti` (JWT ID) per token for blacklisting
- On logout: add `jti` to `IMemoryCache` with TTL = remaining token lifetime
- Check blacklist in `JwtBearerEvents.OnTokenValidated`
- For SignalR: extract token from `access_token` query parameter in `OnMessageReceived`
- Store JWT secret in `appsettings.json` (development) / user-secrets (production), minimum 32 characters

**NuGet**: `Microsoft.AspNetCore.Authentication.JwtBearer`

## 4. File Upload (Resume PDF/DOCX, ≤10MB)

**Decision**: Buffered upload via `IFormFile`, local filesystem storage outside web root, two-layer file type validation (extension + magic bytes).

**Rationale**: 10MB is small enough for buffered upload (streaming only needed for 100MB+). Local filesystem is simplest for a graduation project. Two-layer validation prevents spoofed file types.

**Alternatives Considered**:
- Streaming upload: Rejected — unnecessary for 10MB files, adds significant complexity.
- Azure Blob Storage: Rejected — adds cloud dependency for graduation project. Can be swapped in later via `IFileStorageService` interface.
- Database BLOB storage: Rejected — storing 10MB files in SQL Server degrades DB performance.

**Key Implementation Notes**:
- Configure Kestrel: `MaxRequestBodySize = 11_000_000` and `FormOptions.MultipartBodyLengthLimit = 11_000_000`
- Generate random filenames with `Path.GetRandomFileName()` + original extension
- Validate extensions (`.pdf`, `.docx` whitelist) AND magic bytes (PDF: `%PDF`, DOCX/ZIP: `PK..`)
- Store files in configured path (e.g., `Uploads/Resumes/`), NOT under `wwwroot`
- Serve files through authenticated controller action, not direct file access
- `IFileStorageService` interface in Services.Abstractions, implementation in Persistence/Services

## 5. SignalR + WebRTC (Live Interviews)

**Decision**: SignalR as signaling server only. WebRTC handles peer-to-peer audio/video entirely on the frontend. Backend manages room join/leave, SDP exchange, and ICE candidate forwarding.

**Rationale**: This is the standard architecture for browser-based video calling. SignalR handles the lightweight signaling messages; media never touches the server. No additional infrastructure needed beyond SignalR (already in ASP.NET Core shared framework).

**Alternatives Considered**:
- Media server (Janus, mediasoup): Rejected — massive infrastructure overhead for 1-to-1 interviews.
- Third-party video SDK (Twilio, Agora): Rejected — adds cost and external dependency.
- Pure WebSocket: Rejected — SignalR provides automatic reconnection, fallbacks, and integrates with ASP.NET auth.

**Key Implementation Notes**:
- `InterviewHub`: methods for `JoinRoom`, `SendOffer`, `SendAnswer`, `SendIceCandidate`
- Use SignalR Groups scoped to interview ID
- JWT auth for hubs: extract `access_token` from query string in `JwtBearerEvents.OnMessageReceived`
- Separate `NotificationHub` for real-time in-app notifications
- STUN server: use Google's free `stun:stun.l.google.com:19302` — TURN not needed for graduation project
- Map hubs: `app.MapHub<InterviewHub>("/hubs/interview")`, `app.MapHub<NotificationHub>("/hubs/notifications")`

**NuGet**: None — SignalR is included in ASP.NET Core shared framework

## 6. AI Microservice Communication

**Decision**: `IHttpClientFactory` with Typed Client pattern + `Microsoft.Extensions.Http.Resilience` for retry/circuit-breaker.

**Rationale**: Typed clients provide clean separation and testability. The resilience library (successor to deprecated Polly HTTP extensions) provides production-grade retry and circuit breaker with minimal code.

**Alternatives Considered**:
- Raw `HttpClient`: Rejected — causes socket exhaustion and lacks resilience.
- gRPC: Rejected per PRD — HTTP REST is simpler and matches the Python FastAPI service interface.
- Message queue (RabbitMQ): Rejected — adds infrastructure complexity; HTTP with retry is sufficient for synchronous scoring.

**Key Implementation Notes**:
- `IAiServiceClient` interface in Services.Abstractions with methods: `ScoreResumeAsync`, `ExtractSkillsAsync`, `TokenizeJobAsync`, `GenerateAssessmentAsync`, `StartVoiceInterviewAsync`, `GenerateCvAsync`
- `AiServiceClient` implementation in Persistence/Services
- Resilience pipeline: Retry (3 attempts, exponential backoff) → Circuit Breaker (50% failure, 30s break) → Timeout (45s)
- AI service URL in `appsettings.json` under `AiService:BaseUrl`
- When circuit breaker is open: store application with `MatchScore = null`, queue for retry (per FR-034)
- Set generous timeout (60s on HttpClient) — AI inference can be slow

**NuGet**: `Microsoft.Extensions.Http.Resilience`

## 7. CSV Export

**Decision**: CsvHelper with in-memory `MemoryStream` approach. Streaming is unnecessary for the expected 500-row scale.

**Rationale**: CsvHelper is the de facto standard for CSV in .NET. MemoryStream approach is simple and meets the SC-007 requirement (500+ applicants in <10s) easily.

**Alternatives Considered**:
- Manual string concatenation: Rejected — error-prone for escaping, quoting, and special characters.
- Streaming response: Rejected — adds complexity, unnecessary for 500 rows.
- Excel export (EPPlus): Rejected — spec explicitly calls for CSV.

**Key Implementation Notes**:
- Always use `CultureInfo.InvariantCulture` to prevent locale-dependent delimiters
- Use `[Name("...")]` attributes on export DTOs for human-readable column headers
- Set `Content-Disposition: attachment` header
- Don't forget `memoryStream.Position = 0` before returning

**NuGet**: `CsvHelper`

## 8. Global Error Handling

**Decision**: Custom `GlobalExceptionHandler` middleware that catches domain exceptions and maps them to appropriate HTTP status codes.

**Rationale**: Centralizes error handling, ensures consistent error response format, and prevents stack traces from leaking in production.

**Key Implementation Notes**:
- `NotFoundException` → 404, `BadRequestException` → 400, `ForbiddenException` → 403
- Return a consistent JSON error envelope: `{ statusCode, message, details? }`
- Log exceptions with structured logging (built-in `ILogger`)
- In development: include stack trace in response; in production: omit it

## NuGet Package Summary

| Package | Project | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Persistence | Identity + EF integration |
| `Microsoft.EntityFrameworkCore.SqlServer` | Persistence | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` | IES.api | EF migrations CLI |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | IES.api | JWT authentication |
| `Microsoft.Extensions.Http.Resilience` | Persistence | HTTP client resilience |
| `CsvHelper` | Presentation | CSV export |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | Services | DTO mapping |

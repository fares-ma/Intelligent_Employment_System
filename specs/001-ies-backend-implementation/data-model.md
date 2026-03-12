# Data Model: IES Backend

**Feature**: `001-ies-backend-implementation` | **Date**: 2026-03-04 | **Spec**: [spec.md](spec.md)

## Inheritance Strategy

**TPH (Table-Per-Hierarchy)**: `ApplicationUser`, `CandidateUser`, and `Recruiter` share one database table (`AspNetUsers`) with a `UserType` discriminator column. Subtype-specific columns are nullable.

---

## Entities

### 1. ApplicationUser *(abstract base — inherits IdentityUser)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | string (GUID) | PK, auto-generated | Inherited from IdentityUser |
| Email | string | Required, unique, max 256 | Inherited from IdentityUser |
| PhoneNumber | string? | max 20 | Inherited from IdentityUser |
| PasswordHash | string | Required | Inherited from IdentityUser |
| FirstName | string | Required, max 100 | |
| LastName | string | Required, max 100 | |
| Gender | int (enum) | Required | See Gender enum |
| DateOfBirth | DateTime? | | |
| ProfilePicturePath | string? | max 500 | Filesystem path |
| IsActive | bool | Default: true | Soft-delete flag |
| CreatedAt | DateTime | Required, auto-set | UTC |
| UpdatedAt | DateTime? | | UTC |
| UserType | string | Discriminator | "Candidate" or "Recruiter" |

**Relationships**: None directly — relationships defined on subtypes.

---

### 2. CandidateUser *(inherits ApplicationUser)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| JobTitle | string? | max 200 | Current/desired job title |
| Summary | string? | max 2000 | Professional summary |
| YearsOfExperience | int? | ≥ 0 | |
| CareerLevel | int? (enum) | | See JobLevel enum |
| LinkedInUrl | string? | max 500 | |
| PortfolioUrl | string? | max 500 | |
| Address | string? | max 500 | |
| City | string? | max 100 | |
| Country | string? | max 100 | |

**Relationships**:
- Has many `CandidateSkill` (M:N with Skill via junction)
- Has many `Resume`
- Has many `JobApplication`
- Has many `SavedJob`
- Has many `CandidateAssessment`
- Has many `CandidateEducation`
- Has many `CandidateExperience`

---

### 3. Recruiter *(inherits ApplicationUser)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| CompanyId | int | Required, FK | |
| RecruiterRole | int (enum) | Required, default: Standard | See UserRole enum |

**Relationships**:
- Belongs to one `Company`
- Has many `JobPost` (as creator)

---

### 4. Company

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| Name | string | Required, max 200 | |
| Industry | string? | max 100 | |
| Website | string? | max 500 | |
| TaxNumber | string | Required, unique, max 50 | Immutable after creation |
| PhoneNumber | string? | max 20 | |
| Description | string? | max 2000 | |
| LogoPath | string? | max 500 | |
| IsActive | bool | Default: true | |
| CreatedAt | DateTime | Required, auto-set | UTC |
| UpdatedAt | DateTime? | | UTC |

**Relationships**:
- Has many `Recruiter`
- Has many `JobPost`
- Has many `CompanyInviteCode`

**Validation Rules**:
- TaxNumber is immutable once set (cannot be updated)
- TaxNumber must be unique across all companies

---

### 5. CompanyInviteCode

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| CompanyId | int | Required, FK | |
| Code | string | Required, unique, length 6 | Alphanumeric, e.g. "XK7M2P" |
| MaxUses | int | Required, default: 5 | |
| CurrentUses | int | Required, default: 0 | |
| ExpiresAt | DateTime | Required | UTC |
| CreatedByRecruiterId | string | Required, FK | Admin who generated |
| IsActive | bool | Default: true | Can be revoked |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `Company`
- Belongs to one `Recruiter` (creator)

**Validation Rules**:
- Code is 6 uppercase alphanumeric characters
- Only Admin Recruiters can generate codes
- Code is invalid if IsActive = false, CurrentUses ≥ MaxUses, or ExpiresAt < now

---

### 6. JobPost

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| Title | string | Required, max 200 | |
| Description | string | Required, max 5000 | |
| AiProcessedDescription | string? | max 5000 | AI-extracted structured data |
| Location | string? | max 200 | |
| JobType | int (enum) | Required | FullTime, PartTime, Contract, Internship |
| WorkLocation | int (enum) | Required | OnSite, Remote, Hybrid |
| CareerLevel | int (enum) | Required | See JobLevel enum |
| SalaryMin | decimal? | ≥ 0 | |
| SalaryMax | decimal? | ≥ SalaryMin | |
| Currency | string? | max 10, default "EGP" | |
| ExpiryDate | DateTime? | | Job auto-inactive after this date |
| IsPublished | bool | Default: false | Draft vs published |
| IsActive | bool | Default: true | |
| CompanyId | int | Required, FK | |
| CreatedByRecruiterId | string | Required, FK | |
| CreatedAt | DateTime | Required, auto-set | UTC |
| UpdatedAt | DateTime? | | UTC |
| DeletedAt | DateTime? | | Soft delete timestamp (UTC) |
| DeletedBy | string? | FK → ApplicationUser | Null = system, userId = user |

**Relationships**:
- Belongs to one `Company`
- Belongs to one `Recruiter` (creator)
- Has many `JobPostSkill` (M:N with Skill via junction)
- Has many `JobApplication`
- Has many `Assessment`

**Validation Rules**:
- SalaryMax ≥ SalaryMin when both are provided
- Only published + active + non-expired + non-deleted jobs appear in public listings
- Soft-deleted jobs are hidden from search but visible to existing applicants

---

### 7. Resume

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| CandidateId | string | Required, FK | |
| OriginalFileName | string | Required, max 255 | User's uploaded filename |
| StoredFilePath | string | Required, max 500 | Server filesystem path |
| FileType | string | Required, max 10 | "pdf" or "docx" |
| FileSizeBytes | long | Required, ≤ 10MB | |
| ExperienceSummary | string? | max 2000 | AI-extracted |
| EducationSummary | string? | max 2000 | AI-extracted |
| ActivitiesSummary | string? | max 2000 | AI-extracted |
| AiGeneratedCvPath | string? | max 500 | Path to AI-generated CV |
| IsDefault | bool | Default: false | Primary resume for applications |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `CandidateUser`
- Has many `ResumeSkill` (M:N with Skill via junction)
- Referenced by many `JobApplication` (as the resume submitted)

---

### 8. JobApplication

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| CandidateId | string | Required, FK | |
| JobPostId | int | Required, FK | |
| ResumeId | int | Required, FK | Resume used for this application |
| Status | int (enum) | Required, default: Pending | See ApplicationStatus enum |
| MatchScore | decimal? | 0.00–100.00, precision(5,2) | Null = processing, 0.0 = calculated zero |
| MatchReport | string? | max 5000 | AI-generated analysis |
| RecruiterRating | int? | 1–5 | Set by recruiter |
| RecruiterNotes | string? | max 2000 | |
| AppliedAt | DateTime | Required, auto-set | UTC |
| UpdatedAt | DateTime? | | UTC |
| DeletedAt | DateTime? | | Soft delete timestamp (UTC) |
| DeletedBy | string? | FK → ApplicationUser | Null = system, userId = user |

**Relationships**:
- Belongs to one `CandidateUser`
- Belongs to one `JobPost`
- Belongs to one `Resume`
- Has many `CandidateAssessment`
- Has many `Interview`

**Validation Rules**:
- Unique constraint on (CandidateId, JobPostId) — one application per candidate per job
- RecruiterRating must be 1–5 if provided
- Status transitions follow strict pipeline (see State Transitions below)

---

### 9. Skill

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| Name | string | Required, unique, max 100 | Normalized lowercase |
| Category | int? (enum) | | See SkillCategory enum |

**Relationships**:
- Has many `CandidateSkill` (M:N with CandidateUser)
- Has many `JobPostSkill` (M:N with JobPost)
- Has many `ResumeSkill` (M:N with Resume)

---

### 10. Assessment

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| JobPostId | int | Required, FK | |
| Title | string | Required, max 200 | |
| Description | string? | max 2000 | |
| Type | int (enum) | Required | See AssessmentType enum |
| TimeLimitMinutes | int | Required, > 0 | |
| TotalScore | int | Required, > 0 | Sum of question points |
| IsAiGenerated | bool | Default: false | |
| IsActive | bool | Default: true | |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `JobPost`
- Has many `Question`
- Has many `CandidateAssessment`

---

### 11. Question

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| AssessmentId | int | Required, FK | |
| Text | string | Required, max 2000 | |
| Type | int (enum) | Required | See QuestionType enum |
| Options | string? | max 4000 | JSON array for MCQ choices |
| CorrectAnswer | string? | max 2000 | For auto-grading |
| Points | int | Required, > 0 | |
| OrderIndex | int | Required | Display order |

**Relationships**:
- Belongs to one `Assessment`

---

### 12. Interview

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| JobApplicationId | int | Required, FK | |
| InterviewType | int (enum) | Required | See InterviewType enum |
| Status | int (enum) | Required, default: Scheduled | See InterviewStatus enum |
| ScheduledAt | DateTime | Required | UTC |
| DurationMinutes | int | Required, default: 30 | |
| MeetingLink | string? | max 500 | For live interviews (WebRTC provider) |
| AiQuestions | string? | max 5000 | JSON — AI-generated text questions |
| AiAnswers | string? | max 10000 | JSON — Candidate's written answers |
| Score | decimal? | 0.00–100.00, precision(5,2) | |
| FeedbackNotes | string? | max 2000 | Recruiter or AI feedback |
| CompletedAt | DateTime? | | UTC |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `JobApplication`

---

### 13. SavedJob *(junction entity)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| CandidateId | string | PK (composite), FK | |
| JobPostId | int | PK (composite), FK | |
| SavedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `CandidateUser`
- Belongs to one `JobPost`

---

### 14. CandidateSkill *(junction entity)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| CandidateId | string | PK (composite), FK | |
| SkillId | int | PK (composite), FK | |
| Level | int (enum) | Required, default: Beginner | See SkillLevel enum |

**Relationships**:
- Belongs to one `CandidateUser`
- Belongs to one `Skill`

---

### 15. JobPostSkill *(junction entity)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| JobPostId | int | PK (composite), FK | |
| SkillId | int | PK (composite), FK | |
| IsRequired | bool | Default: true | Required vs nice-to-have |
| RequiredLevel | int? (enum) | | See SkillLevel enum. Null = any level |

**Relationships**:
- Belongs to one `JobPost`
- Belongs to one `Skill`

---

### 16. ResumeSkill *(junction entity)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| ResumeId | int | PK (composite), FK | |
| SkillId | int | PK (composite), FK | |
| Proficiency | string? | max 50 | e.g., "Advanced", "Beginner" |

**Relationships**:
- Belongs to one `Resume`
- Belongs to one `Skill`

---

### 17. CandidateAssessment *(junction entity)*

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| CandidateId | string | Required, FK | |
| AssessmentId | int | Required, FK | |
| JobApplicationId | int | Required, FK | |
| Answers | string? | max 10000 | JSON — submitted answers |
| Score | decimal? | precision(5,2) | Auto-graded or manual |
| StartedAt | DateTime? | | UTC |
| SubmittedAt | DateTime? | | UTC |
| IsCompleted | bool | Default: false | |

**Relationships**:
- Belongs to one `CandidateUser`
- Belongs to one `Assessment`
- Belongs to one `JobApplication`

**Validation Rules**:
- Unique constraint on (CandidateId, AssessmentId) — one attempt per candidate per assessment
- SubmittedAt must be within TimeLimitMinutes of StartedAt
- Cannot submit after assessment time window expires

---

### 18. CandidateEducation

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| CandidateId | string | Required, FK | |
| Degree | string | Required, max 200 | e.g., "BSc", "MSc", "PhD" |
| FieldOfStudy | string | Required, max 200 | e.g., "Computer Science" |
| Institution | string | Required, max 300 | |
| GraduationYear | int | Required | 4-digit year |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `CandidateUser`

---

### 19. CandidateExperience

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| CandidateId | string | Required, FK | |
| JobTitle | string | Required, max 200 | |
| Company | string | Required, max 200 | |
| Description | string? | max 500 | |
| StartDate | DateTime | Required | |
| EndDate | DateTime? | | Null = currently employed |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `CandidateUser`

---

### 20. Notification

| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, auto-increment | |
| UserId | string | Required, FK | References ApplicationUser |
| Type | string | Required, max 100 | e.g., "ApplicationSubmitted", "StatusChanged", "InterviewScheduled" |
| Title | string | Required, max 200 | Notification headline |
| Message | string | Required, max 1000 | Notification body |
| TargetUrl | string? | max 500 | Deep-link for frontend navigation |
| IsRead | bool | Default: false | |
| CreatedAt | DateTime | Required, auto-set | UTC |

**Relationships**:
- Belongs to one `ApplicationUser` (via UserId FK)

**Validation Rules**:
- UserId must reference an existing ApplicationUser
- Type should match a known notification type constant

---

## Enums

### Gender
| Value | Name |
|---|---|
| 0 | Male |
| 1 | Female |

### UserRole *(Recruiter role within company)*
| Value | Name | Permissions |
|---|---|---|
| 0 | Admin | Full company management, invite codes, transfer admin, all recruiter actions |
| 1 | Standard | Post jobs, manage own jobs, manage applicants, schedule interviews |

### ApplicationStatus
| Value | Name |
|---|---|
| 0 | Pending |
| 1 | UnderReview |
| 2 | Assessment |
| 3 | Interview |
| 4 | Accepted |
| 5 | Rejected |
| 6 | Withdrawn |

### InterviewType
| Value | Name |
|---|---|
| 0 | AI |
| 1 | Live |

### InterviewStatus
| Value | Name |
|---|---|
| 0 | Scheduled |
| 1 | InProgress |
| 2 | Completed |
| 3 | Cancelled |

### JobType
| Value | Name |
|---|---|
| 0 | FullTime |
| 1 | PartTime |
| 2 | Contract |
| 3 | Internship |

### WorkLocation
| Value | Name |
|---|---|
| 0 | OnSite |
| 1 | Remote |
| 2 | Hybrid |

### JobLevel
| Value | Name |
|---|---|
| 0 | Entry |
| 1 | Junior |
| 2 | Mid |
| 3 | Senior |
| 4 | Lead |
| 5 | Manager |
| 6 | Director |
| 7 | Executive |

### QuestionType
| Value | Name |
|---|---|
| 0 | MCQ |
| 1 | TrueFalse |
| 2 | OpenEnded |
| 3 | Coding |

### AssessmentType
| Value | Name |
|---|---|
| 0 | Technical |
| 1 | Personality |
| 2 | Mixed |

### SkillCategory
| Value | Name |
|---|---|
| 0 | Technical |
| 1 | Language |
| 2 | Soft |
| 3 | Tool |
| 4 | Other |

### SkillLevel *(proficiency level)*
| Value | Name |
|---|---|
| 1 | Beginner |
| 2 | Intermediate |
| 3 | Expert |

---

## State Transitions

### ApplicationStatus Pipeline

```text
                                                             ┌──→ Accepted
Pending ──→ UnderReview ──→ Assessment ──→ Interview ──┤
  │  │           │               │             │       └──→ Rejected
  │  └──→ Withdrawn (by candidate)
  └──→ Rejected  └──→ Rejected   └──→ Rejected └──→ Rejected
```

**Rules**:
- Strictly sequential — no stage skipping allowed
- No backwards movement (cannot go from Interview back to Assessment)
- Rejection is allowed from any active stage
- Withdrawal is only allowed by the candidate when status = Pending (terminal state)
- Once Accepted, Rejected, or Withdrawn, the status is terminal (no further changes)
- Re-application to the same job after Withdrawn is NOT allowed

**Valid Transitions**:
| From | To (allowed) |
|---|---|
| Pending | UnderReview, Rejected, Withdrawn |
| UnderReview | Assessment, Rejected |
| Assessment | Interview, Rejected |
| Interview | Accepted, Rejected |
| Accepted | *(terminal)* |
| Rejected | *(terminal)* |
| Withdrawn | *(terminal)* |

### InterviewStatus Flow

```text
Scheduled ──→ InProgress ──→ Completed
    │
    └──→ Cancelled
```

**Valid Transitions**:
| From | To (allowed) |
|---|---|
| Scheduled | InProgress, Cancelled |
| InProgress | Completed |
| Completed | *(terminal)* |
| Cancelled | *(terminal)* |

---

## Entity Relationship Diagram (text)

```text
Company (1) ───── (M) Recruiter
Company (1) ───── (M) JobPost
Company (1) ───── (M) CompanyInviteCode
Recruiter (1) ─── (M) JobPost (creator)
Recruiter (1) ─── (M) CompanyInviteCode (creator)

CandidateUser (M) ── CandidateSkill ── (M) Skill
JobPost (M) ──────── JobPostSkill ───── (M) Skill
Resume (M) ────────── ResumeSkill ────── (M) Skill

CandidateUser (1) ── (M) Resume
CandidateUser (1) ── (M) JobApplication
CandidateUser (1) ── (M) CandidateEducation
CandidateUser (1) ── (M) CandidateExperience
CandidateUser (M) ── SavedJob ── (M) JobPost

JobPost (1) ──────── (M) JobApplication
JobApplication (1) ── Resume (via FK)

JobPost (1) ──────── (M) Assessment
Assessment (1) ───── (M) Question

JobApplication (1) ── (M) Interview
JobApplication (1) ── (M) CandidateAssessment
CandidateUser (1) ── (M) CandidateAssessment
Assessment (1) ────── (M) CandidateAssessment

ApplicationUser (1) ── (M) Notification
```

---

## Indexes

| Table | Columns | Type | Rationale |
|---|---|---|---|
| JobApplication | (CandidateId, JobPostId) | Unique | Prevent duplicate applications |
| CandidateAssessment | (CandidateId, AssessmentId) | Unique | One attempt per assessment |
| Company | TaxNumber | Unique | Business rule |
| Skill | Name | Unique | Prevent duplicates |
| CompanyInviteCode | Code | Unique | Fast lookup by invite code |
| JobPost | (IsPublished, IsActive, ExpiryDate, DeletedAt) | Composite | Public listing filter queries |
| JobPost | CompanyId | Non-unique | Company jobs lookup |
| JobApplication | JobPostId | Non-unique | Applicants per job |
| JobApplication | CandidateId | Non-unique | Candidate's applications |
| SavedJob | CandidateId | Non-unique | Candidate's saved jobs |
| Notification | UserId | Non-unique | User's notifications lookup |
| Notification | (UserId, IsRead) | Composite | Unread notifications filter |
| CandidateEducation | CandidateId | Non-unique | Candidate's education lookup |
| CandidateExperience | CandidateId | Non-unique | Candidate's experience lookup |

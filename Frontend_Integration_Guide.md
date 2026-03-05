# 🚀 Intelligent Employment System (IES) — Frontend Integration Guide

> **Version**: 2.0 | **Last Updated**: 2026-03-05  
> **Backend Stack**: .NET 10, ASP.NET Core, SQL Server, SignalR, JWT  
> **Target Frontend**: Angular / React / Vue (TypeScript)

This document provides **everything** the frontend team needs to integrate with the IES backend — endpoints, request/response bodies, enums, business rules, real-time features, and code examples.

---

## 📖 Table of Contents

1. [Project Overview](#-1-project-overview)
2. [Development Setup](#-2-development-setup)
3. [Authentication & Authorization](#-3-authentication--authorization)
4. [HTTP Client Setup (Axios / Fetch)](#-4-http-client-setup)
5. [TypeScript Interfaces (All DTOs)](#-5-typescript-interfaces)
6. [Enums Reference](#-6-enums-reference)
7. [API Endpoints — Detailed](#-7-api-endpoints--detailed)
8. [Real-Time Features (SignalR)](#-8-real-time-features-signalr)
9. [File Uploads Guide](#-9-file-uploads-guide)
10. [Business Rules & Edge Cases](#-10-business-rules--edge-cases)
11. [State Machines](#-11-state-machines)
12. [Error Handling Reference](#-12-error-handling-reference)

---

## 📖 1. Project Overview

IES is an AI-powered recruitment platform connecting **Candidates** and **Recruiters**. Core features:

- AI matching (Resume vs. Job Description)
- AI-generated assessments & scoring
- AI voice interviews with transcript analysis
- Real-time live video interviews (WebRTC + SignalR)
- Real-time notifications

### User Roles

| Role | Description | Capabilities |
|------|-------------|-------------|
| **Candidate** | Job seeker | Build profile, upload resumes, apply to jobs, take assessments, do interviews |
| **Recruiter (Admin)** | First recruiter registered for a company — auto-assigned | Full company control: edit company, manage recruiters, post jobs, manage applicants |
| **Recruiter (Standard)** | Subsequent recruiters | Post jobs, manage applicants, schedule interviews |
| **Recruiter (Junior)** | Limited recruiter | View applicants and ratings only — **cannot** post jobs |

> ⚠️ The first recruiter to register for a company automatically gets `Admin` role. All subsequent recruiters get `Standard` by default.

---

## 🛠 2. Development Setup

### Backend URLs (Development)

| Protocol | URL |
|----------|-----|
| HTTPS | `https://localhost:7001` |
| HTTP | `http://localhost:5001` |

### Base API Path

```
{BASE_URL}/api/{resource}
```

### CORS

The backend allows requests from `http://localhost:4200` (Angular default) with:
- All methods (GET, POST, PUT, PATCH, DELETE)
- All headers
- Credentials allowed (required for SignalR)

> If your dev server runs on a different port, ask the backend team to add it to `appsettings.Development.json → Cors:AllowedOrigins`.

### Swagger UI

Interactive API documentation is available at:
```
https://localhost:7001/
```
Use the **Authorize** button (🔒) to enter your JWT token and test endpoints directly.

---

## 🔐 3. Authentication & Authorization

### Mechanism
- **JWT Bearer Token** — 24-hour expiration, **no refresh token**
- Token is passed in the `Authorization` header: `Bearer <token>`
- Logout blacklists the token server-side (in-memory cache with remaining TTL)

### Login Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-03-06T14:30:00Z",
  "userId": "a1b2c3d4-...",
  "email": "user@example.com",
  "role": "Candidate",
  "userType": "Candidate"
}
```

> **`role`** values: `"Candidate"` | `"Recruiter"` | `"AdminRecruiter"`  
> **`userType`** values: `"Candidate"` | `"Recruiter"`  
> The difference: `role` includes the recruiter's permission level, `userType` is the base type.

### Frontend Auth Flow

```
1. POST /api/auth/login → Store token + expiresAt in localStorage/cookie
2. All protected requests → Add header: Authorization: Bearer <token>
3. On 401 response → Redirect to login page
4. POST /api/auth/logout → Clear token locally
5. Check expiresAt client-side → Auto-logout before expiry
```

### Security Notes
- Login returns generic `"Invalid credentials"` — **never** hints whether email or password is wrong
- Forgot password always returns `200 OK` with generic message — **never** confirms email exists
- Passwords are never returned in any API response

---

## 🔧 4. HTTP Client Setup

### Recommended Axios Configuration

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7001/api',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true, // Required for SignalR CORS
});

// ── Request Interceptor: Attach JWT ──
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('ies_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ── Response Interceptor: Handle Errors ──
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const errorBody: ApiErrorResponse = error.response?.data;

    if (status === 401) {
      // Token expired or invalid → redirect to login
      localStorage.removeItem('ies_token');
      window.location.href = '/login';
    }

    if (status === 503) {
      // AI service unavailable → show graceful message
      console.warn('AI service temporarily unavailable');
    }

    return Promise.reject(errorBody || error);
  }
);

export default api;
```

---

## 📦 5. TypeScript Interfaces

### Global Wrappers

```typescript
/** All paginated endpoints return this wrapper */
interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/** All error responses follow this shape */
interface ApiErrorResponse {
  statusCode: number;
  message: string;
  details?: string; // Validation details or stack trace (dev only)
}

/** Standard pagination query params */
interface PaginationParams {
  page?: number;      // Default: 1
  pageSize?: number;  // Default: 10, Max: 50
}
```

### Auth DTOs

```typescript
interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;        // Min 8 chars, upper+lower+digit+special
  phoneNumber?: string;
  gender: Gender;           // 0=Male, 1=Female
  dateOfBirth?: string;     // ISO 8601
  userType: 'Candidate' | 'Recruiter';
  companyId?: number;       // REQUIRED if userType = "Recruiter"
}

interface RegisterResponse {
  userId: string;
  email: string;
  userType: string;
}

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  expiresAt: string;        // ISO 8601
  userId: string;
  email: string;
  role: 'Candidate' | 'Recruiter' | 'AdminRecruiter';
  userType: 'Candidate' | 'Recruiter';
}

interface ForgotPasswordRequest {
  email: string;
}

interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
```

### Candidate DTOs

```typescript
interface CandidateProfileDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  gender: Gender;
  dateOfBirth?: string;
  profilePicturePath?: string;
  jobTitle?: string;
  summary?: string;
  yearsOfExperience?: number;
  careerLevel?: JobLevel;
  linkedInUrl?: string;
  portfolioUrl?: string;
  address?: string;
  city?: string;
  country?: string;
  skills: SkillDto[];
  resumes: ResumeSummaryDto[];
}

interface UpdateCandidateProfileRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  gender?: Gender;
  dateOfBirth?: string;
  jobTitle?: string;
  summary?: string;
  yearsOfExperience?: number;
  careerLevel?: JobLevel;
  linkedInUrl?: string;
  portfolioUrl?: string;
  address?: string;
  city?: string;
  country?: string;
}

interface UpdateSkillsRequest {
  skillIds: number[];
}

interface ResumeSummaryDto {
  id: number;
  originalFileName: string;
  fileType: string;
  fileSizeBytes: number;
  isDefault: boolean;
  aiGeneratedCvPath?: string;
  createdAt: string;
}

interface CandidateApplicationDto {
  id: number;
  jobPost: {
    id: number;
    title: string;
    companyName: string;
    location?: string;
  };
  status: ApplicationStatus;
  matchScore?: number;       // null = pending AI scoring
  appliedAt: string;
  updatedAt?: string;
}

interface SavedJobDto {
  jobPostId: number;
  title: string;
  company: { id: number; name: string; logoPath?: string };
  location?: string;
  jobType: JobType;
  careerLevel: JobLevel;
  savedAt: string;
}

interface CandidateDashboardDto {
  applicationsCount: number;
  savedJobsCount: number;
  upcomingInterviews: InterviewListDto[];
  recentActivity: ActivityDto[];
  aiSuggestedJobs: JobListDto[];
}
```

### Company DTOs

```typescript
interface CompanyDto {
  id: number;
  name: string;
  industry?: string;
  logoPath?: string;
}

interface CompanyDetailDto {
  id: number;
  name: string;
  industry?: string;
  website?: string;
  phoneNumber?: string;
  description?: string;
  logoPath?: string;
  isVerified: boolean;
  jobCount: number;
  createdAt: string;
}

interface CreateCompanyRequest {
  name: string;
  industry?: string;
  website?: string;
  taxNumber: string;        // Immutable after creation
  phoneNumber?: string;
  description?: string;
}

interface UpdateCompanyRequest {
  name?: string;
  industry?: string;
  website?: string;
  phoneNumber?: string;
  description?: string;
  // NOTE: taxNumber CANNOT be updated
}

interface RecruiterListDto {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  recruiterRole: UserRole;
}

interface AddRecruiterRequest {
  userId: string;
  recruiterRole?: UserRole;  // Default: Standard
}

interface CompanyDashboardDto {
  totalApplicants: number;
  activeJobs: number;
  hireRate: number;
  applicationTrends: TrendDataPoint[];
  recentJobPosts: JobListDto[];
  actionRequired: ActionItemDto[];
}
```

### Job DTOs

```typescript
interface JobListDto {
  id: number;
  title: string;
  company: { id: number; name: string; logoPath?: string };
  location?: string;
  jobType: JobType;
  workLocation: WorkLocation;
  careerLevel: JobLevel;
  salaryMin?: number;
  salaryMax?: number;
  currency?: string;
  skills: { id: number; name: string }[];
  applicantsCount: number;
  createdAt: string;
  expiryDate?: string;
}

interface JobDetailDto {
  id: number;
  title: string;
  description: string;
  aiProcessedDescription?: string;
  location?: string;
  jobType: JobType;
  workLocation: WorkLocation;
  careerLevel: JobLevel;
  salaryMin?: number;
  salaryMax?: number;
  currency?: string;          // Default: "EGP"
  expiryDate?: string;
  isPublished: boolean;
  isActive: boolean;
  company: { id: number; name: string; industry?: string; logoPath?: string };
  skills: { id: number; name: string; isRequired: boolean }[];
  applicantsCount: number;
  similarJobs: JobListDto[];
  createdAt: string;
  updatedAt?: string;
}

interface CreateJobRequest {
  title: string;
  description: string;
  location?: string;
  jobType: JobType;
  workLocation: WorkLocation;
  careerLevel: JobLevel;
  salaryMin?: number;
  salaryMax?: number;
  currency?: string;
  expiryDate?: string;
  isPublished: boolean;
  skillIds: number[];
}

interface UpdateJobRequest {
  title?: string;
  description?: string;
  location?: string;
  jobType?: JobType;
  workLocation?: WorkLocation;
  careerLevel?: JobLevel;
  salaryMin?: number;
  salaryMax?: number;
  currency?: string;
  expiryDate?: string;
  skillIds?: number[];
}

/** Job search/filter query parameters */
interface JobSearchParams extends PaginationParams {
  search?: string;          // Free-text search in title/description
  location?: string;
  jobType?: JobType;
  workLocation?: WorkLocation;
  careerLevel?: JobLevel;
  industry?: string;
  salaryMin?: number;
  salaryMax?: number;
  companyId?: number;
  sortBy?: 'createdAt' | 'salaryMin' | 'title';
  sortOrder?: 'asc' | 'desc';
}

interface ApplyJobRequest {
  resumeId: number;
}

interface ApplyJobResponse {
  applicationId: number;
  status: ApplicationStatus;  // Always "Pending"
  appliedAt: string;
}
```

### Applicant DTOs (Recruiter View)

```typescript
interface ApplicantDto {
  applicationId: number;
  candidate: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    jobTitle?: string;
    yearsOfExperience?: number;
  };
  status: ApplicationStatus;
  matchScore?: number;        // null = AI scoring pending
  matchReport?: string;       // AI-generated analysis text
  recruiterRating?: number;   // 1–5 stars
  appliedAt: string;
  updatedAt?: string;
}

interface UpdateStatusRequest {
  newStatus: ApplicationStatus;
}

interface UpdateRatingRequest {
  rating: number;             // 1–5
}

/** Applicant list filter/sort params */
interface ApplicantSearchParams extends PaginationParams {
  status?: ApplicationStatus;
  minScore?: number;
  sortBy?: 'appliedAt' | 'matchScore' | 'recruiterRating';
  sortOrder?: 'asc' | 'desc';
}
```

### Assessment DTOs

```typescript
interface AssessmentDetailDto {
  id: number;
  jobPostId: number;
  title: string;
  description?: string;
  type: AssessmentType;
  timeLimitMinutes: number;
  totalScore: number;
  isAiGenerated: boolean;
  isActive: boolean;
  questions: QuestionDto[];
  createdAt: string;
}

interface QuestionDto {
  id: number;
  text: string;
  type: QuestionType;
  options?: string;           // JSON array string for MCQ
  correctAnswer?: string;     // ⚠️ HIDDEN from candidates, only visible to recruiters
  points: number;
  orderIndex: number;
}

interface CreateAssessmentRequest {
  jobPostId: number;
  title: string;              // Max 200 chars
  description?: string;       // Max 2000 chars
  type: AssessmentType;
  timeLimitMinutes: number;   // Must be > 0
  isAiGenerated?: boolean;
  questions: CreateQuestionRequest[];
}

interface CreateQuestionRequest {
  text: string;               // Max 2000 chars
  type: QuestionType;
  options?: string;           // JSON array string for MCQ
  correctAnswer?: string;     // Max 2000 chars
  points: number;             // Must be > 0
  orderIndex: number;
}

interface AssessmentListDto {
  id: number;
  title: string;
  type: AssessmentType;
  timeLimitMinutes: number;
  totalScore: number;
  isAiGenerated: boolean;
  isActive: boolean;
}

interface StartAssessmentResponse {
  candidateAssessmentId: number;
  startedAt: string;
  deadlineAt: string;         // ⚠️ MUST submit before this time
  questions: QuestionDto[];   // correctAnswer is HIDDEN
}

interface SubmitAssessmentRequest {
  answers: { questionId: number; answer: string }[];
}

interface SubmitAssessmentResponse {
  candidateAssessmentId: number;
  score: number;
  totalScore: number;
  isCompleted: boolean;
}

interface CandidateAssessmentResultDto {
  candidateAssessmentId: number;
  candidate: { id: string; firstName: string; lastName: string; email: string };
  score: number;
  totalScore: number;
  startedAt: string;
  submittedAt?: string;
  isCompleted: boolean;
}
```

### Interview DTOs

```typescript
interface InterviewDetailDto {
  id: number;
  jobApplication: {
    id: number;
    jobTitle: string;
    candidateName: string;
  };
  interviewType: InterviewType;
  status: InterviewStatus;
  scheduledAt: string;
  durationMinutes: number;
  meetingLink?: string;       // Auto-generated for Live interviews
  score?: number;             // 0–100
  feedbackNotes?: string;
  aiTranscript?: string;
  completedAt?: string;
  createdAt: string;
}

interface InterviewListDto {
  id: number;
  jobTitle: string;
  candidateName: string;
  interviewType: InterviewType;
  status: InterviewStatus;
  scheduledAt: string;
  durationMinutes: number;
}

interface ScheduleInterviewRequest {
  jobApplicationId: number;
  interviewType: InterviewType;  // 0=AI, 1=Live
  scheduledAt: string;           // ISO 8601 — must be in the future
  durationMinutes?: number;      // Default: 30
}

/** Interview list filter params */
interface InterviewSearchParams extends PaginationParams {
  status?: InterviewStatus;
  interviewType?: InterviewType;
  upcoming?: boolean;
}

interface StartAiInterviewResponse {
  interviewId: number;
  questions: { text: string; expectedTopics: string[] }[];
  status: 'InProgress';
}

interface CompleteAiInterviewRequest {
  transcript: string;
}

interface CompleteAiInterviewResponse {
  interviewId: number;
  score: number;
  feedback: string;
  status: 'Completed';
}

interface JoinLiveInterviewResponse {
  interviewId: number;
  meetingLink: string;
  signalingHubUrl: string;
  roomId: string;
}

interface CompleteLiveInterviewRequest {
  score?: number;
  feedbackNotes?: string;
}

interface CancelInterviewRequest {
  reason?: string;
}
```

### Notification DTOs

```typescript
interface NotificationDto {
  id: number;
  type: string;
  title: string;
  message: string;
  targetUrl?: string;
  isRead: boolean;
  createdAt: string;
}

interface UnreadCountResponse {
  count: number;
}

interface MarkAllReadResponse {
  updatedCount: number;
}
```

### AI DTOs

```typescript
interface ExtractSkillsRequest {
  text: string;               // Max 5000 chars
}

interface ExtractSkillsResponse {
  skills: { name: string; category: SkillCategory; isRequired: boolean }[];
  summary: string;
}

interface AnalyzeResumeRequest {
  resumeId: number;
  jobPostId: number;
}

interface AnalyzeResumeResponse {
  matchScore: number;
  matchReport: string;
  skillsGap: { skill: string; status: string }[];
}

interface GenerateAssessmentRequest {
  jobPostId: number;
  questionCount?: number;     // Default: 10, Max: 30
  questionTypes?: QuestionType[];
}

interface GenerateAssessmentResponse {
  questions: {
    text: string;
    type: QuestionType;
    options?: string;
    correctAnswer?: string;
    points: number;
  }[];
}

interface GenerateCvRequest {
  resumeId: number;
}

interface GenerateCvResponse {
  cvPath: string;
  summary: string;
}

interface GenerateInterviewQuestionsRequest {
  jobPostId: number;
  questionCount?: number;     // Default: 5, Max: 15
}

interface GenerateInterviewQuestionsResponse {
  questions: { text: string; expectedTopics: string[] }[];
}

interface SkillDto {
  id: number;
  name: string;
  category?: SkillCategory;
}
```

---

## 🎯 6. Enums Reference

Create a constants file matching these values **exactly**:

```typescript
// ── enums.ts ──

export enum Gender {
  Male = 0,
  Female = 1,
}

export enum UserRole {
  Admin = 0,
  Standard = 1,
  Junior = 2,
}

export enum ApplicationStatus {
  Pending = 0,
  UnderReview = 1,
  Assessment = 2,
  Interview = 3,
  Accepted = 4,
  Rejected = 5,
}

export enum InterviewType {
  AI = 0,
  Live = 1,
}

export enum InterviewStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
}

export enum JobType {
  FullTime = 0,
  PartTime = 1,
  Contract = 2,
  Internship = 3,
}

export enum WorkLocation {
  OnSite = 0,
  Remote = 1,
  Hybrid = 2,
}

export enum JobLevel {
  Entry = 0,
  Junior = 1,
  Mid = 2,
  Senior = 3,
  Lead = 4,
  Manager = 5,
  Director = 6,
  Executive = 7,
}

export enum QuestionType {
  MCQ = 0,
  TrueFalse = 1,
  OpenEnded = 2,
  Coding = 3,
}

export enum AssessmentType {
  Technical = 0,
  Personality = 1,
  Mixed = 2,
}

export enum SkillCategory {
  Technical = 0,
  Language = 1,
  Soft = 2,
  Tool = 3,
  Other = 4,
}
```

### Human-Readable Labels (for UI display)

```typescript
export const ApplicationStatusLabels: Record<ApplicationStatus, string> = {
  [ApplicationStatus.Pending]: 'Pending',
  [ApplicationStatus.UnderReview]: 'Under Review',
  [ApplicationStatus.Assessment]: 'Assessment',
  [ApplicationStatus.Interview]: 'Interview',
  [ApplicationStatus.Accepted]: 'Accepted',
  [ApplicationStatus.Rejected]: 'Rejected',
};

export const JobTypeLabels: Record<JobType, string> = {
  [JobType.FullTime]: 'Full Time',
  [JobType.PartTime]: 'Part Time',
  [JobType.Contract]: 'Contract',
  [JobType.Internship]: 'Internship',
};

export const WorkLocationLabels: Record<WorkLocation, string> = {
  [WorkLocation.OnSite]: 'On-Site',
  [WorkLocation.Remote]: 'Remote',
  [WorkLocation.Hybrid]: 'Hybrid',
};

export const JobLevelLabels: Record<JobLevel, string> = {
  [JobLevel.Entry]: 'Entry Level',
  [JobLevel.Junior]: 'Junior',
  [JobLevel.Mid]: 'Mid-Level',
  [JobLevel.Senior]: 'Senior',
  [JobLevel.Lead]: 'Lead',
  [JobLevel.Manager]: 'Manager',
  [JobLevel.Director]: 'Director',
  [JobLevel.Executive]: 'Executive',
};
```

---

## 🔗 7. API Endpoints — Detailed

> **Base Path**: `/api`  
> **Content-Type**: `application/json` (unless file upload → `multipart/form-data`)  
> **Dates**: Always **ISO 8601** (e.g., `2026-03-05T14:30:00Z`)  
> **Currency**: Default `EGP` unless overridden

---

### 🔐 Auth — `/api/auth`

#### `POST /api/auth/register` — Register a new user
```
Auth: Public
Body: RegisterRequest
Response: 201 → RegisterResponse
Errors: 400 (validation) | 409 (email already exists)
```
**Example Request (Candidate):**
```json
{
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "email": "ahmed@example.com",
  "password": "P@ssw0rd!",
  "gender": 0,
  "dateOfBirth": "1995-06-15T00:00:00Z",
  "userType": "Candidate"
}
```
**Example Request (Recruiter):**
```json
{
  "firstName": "Sara",
  "lastName": "Ali",
  "email": "sara@company.com",
  "password": "P@ssw0rd!",
  "gender": 1,
  "userType": "Recruiter",
  "companyId": 1
}
```
> ⚠️ `companyId` is **required** for Recruiter registration. The first recruiter for a company is auto-assigned `Admin` role.

---

#### `POST /api/auth/login` — Login
```
Auth: Public
Body: LoginRequest
Response: 200 → LoginResponse
Errors: 401 ("Invalid credentials" — no field-specific hints)
```

---

#### `POST /api/auth/logout` — Invalidate token
```
Auth: Bearer JWT
Response: 204 No Content
```
> Frontend must also clear the token from local storage.

---

#### `POST /api/auth/forgot-password`
```
Auth: Public
Body: { "email": "string" }
Response: 200 → { "message": "If this email exists, a reset link has been sent." }
```
> Always returns 200 — never reveals whether the email exists (security).

---

#### `POST /api/auth/reset-password`
```
Auth: Public
Body: ResetPasswordRequest
Response: 200 → { "message": "Password reset successfully." }
Errors: 400 (invalid/expired token)
```

---

#### `POST /api/auth/change-password`
```
Auth: Bearer JWT
Body: ChangePasswordRequest
Response: 200 → { "message": "Password changed successfully." }
Errors: 400 (validation / wrong current password) | 401
```

---

### 👤 Candidates — `/api/candidates`

#### `GET /api/candidates/profile` — Get my profile
```
Auth: Candidate
Response: 200 → CandidateProfileDto
```

#### `PUT /api/candidates/profile` — Update my profile
```
Auth: Candidate
Body: UpdateCandidateProfileRequest (all fields optional — partial update)
Response: 200 → CandidateProfileDto
```

#### `PUT /api/candidates/skills` — Replace all my skills
```
Auth: Candidate
Body: { "skillIds": [1, 5, 12, 23] }
Response: 200 → SkillDto[]
```
> This **replaces** the entire skill list — not append. Send the complete set.

#### `POST /api/candidates/resume` — Upload resume
```
Auth: Candidate
Content-Type: multipart/form-data
Body: FormData { file: File, isDefault?: boolean }
Constraints: PDF or DOCX only, max 10MB
Response: 201 → ResumeSummaryDto
Errors: 400 (invalid file type / too large)
```

#### `DELETE /api/candidates/resume/{resumeId}` — Delete resume
```
Auth: Candidate
Response: 204
Errors: 404 (not found)
```

#### `POST /api/candidates/resume/{resumeId}/generate-cv` — AI-generate enhanced CV
```
Auth: Candidate (own resume only)
Response: 200 → { "aiGeneratedCvPath": "string" }
Errors: 404 | 503 (AI unavailable)
```

#### `PUT /api/candidates/profile-picture` — Upload profile picture
```
Auth: Candidate
Content-Type: multipart/form-data
Body: FormData { image: File }
Constraints: JPG or PNG only, max 5MB
Response: 200 → { "profilePicturePath": "string" }
```

#### `GET /api/candidates/applications` — My applications
```
Auth: Candidate
Query: page, pageSize, status? (filter by ApplicationStatus)
Response: 200 → PagedResult<CandidateApplicationDto>
```

#### `GET /api/candidates/saved-jobs` — My saved jobs
```
Auth: Candidate
Query: page, pageSize
Response: 200 → PagedResult<SavedJobDto>
```

#### `POST /api/candidates/saved-jobs/{jobPostId}` — Toggle save/unsave job
```
Auth: Candidate
Response: 200 → { "isSaved": boolean }
```
> Acts as a toggle: if already saved → unsaves it, if not saved → saves it.

#### `GET /api/candidates/dashboard` — Dashboard metrics
```
Auth: Candidate
Response: 200 → CandidateDashboardDto
```

---

### 🏢 Companies — `/api/companies`

#### `POST /api/companies` — Create a company
```
Auth: SuperAdmin (platform-level)
Body: CreateCompanyRequest
Response: 201 → CompanyDetailDto
Errors: 409 (taxNumber already exists)
```

#### `GET /api/companies` — List/search companies
```
Auth: Public
Query: query? (search text), page, pageSize
Response: 200 → PagedResult<CompanyDto>
```

#### `GET /api/companies/{companyId}` — Company details
```
Auth: Public
Response: 200 → CompanyDetailDto
```

#### `PUT /api/companies/{companyId}` — Update company
```
Auth: AdminRecruiter (of this company)
Body: UpdateCompanyRequest
Response: 200 → CompanyDetailDto
```
> ⚠️ `taxNumber` is immutable — cannot be changed after creation.

#### `PUT /api/companies/{companyId}/logo` — Upload company logo
```
Auth: AdminRecruiter
Content-Type: multipart/form-data
Body: FormData { image: File }
Constraints: JPG/PNG, max 5MB
Response: 200 → { "logoPath": "string" }
```

#### `POST /api/companies/{companyId}/recruiters` — Add recruiter to company
```
Auth: AdminRecruiter
Body: AddRecruiterRequest
Response: 200
```

#### `GET /api/companies/{companyId}/recruiters` — List company recruiters
```
Auth: Recruiter (of this company)
Response: 200 → RecruiterListDto[]
```

#### `GET /api/companies/{companyId}/jobs` — Company's job posts
```
Auth: Public
Query: page, pageSize
Response: 200 → PagedResult<JobListDto>
```

#### `GET /api/companies/dashboard` — Company dashboard
```
Auth: Recruiter
Response: 200 → CompanyDashboardDto
```

---

### 📝 Jobs — `/api/jobs`

#### `POST /api/jobs` — Create a job post
```
Auth: Recruiter (Admin or Standard)
Body: CreateJobRequest
Response: 201 → JobDetailDto
```

#### `GET /api/jobs` — Search/browse jobs (public)
```
Auth: Public
Query: JobSearchParams (all optional — search, location, jobType, workLocation,
       careerLevel, industry, salaryMin, salaryMax, companyId,
       sortBy, sortOrder, page, pageSize)
Response: 200 → PagedResult<JobListDto>
```
**Example search:** `GET /api/jobs?search=software&workLocation=1&careerLevel=2&sortBy=createdAt&sortOrder=desc&page=1&pageSize=20`

#### `GET /api/jobs/{jobId}` — Job details
```
Auth: Public (published jobs) / Recruiter (drafts)
Response: 200 → JobDetailDto
```

#### `PUT /api/jobs/{jobId}` — Update job post
```
Auth: Recruiter (creator or Admin)
Body: UpdateJobRequest (all fields optional)
Response: 200 → JobDetailDto
```

#### `DELETE /api/jobs/{jobId}` — Delete job post
```
Auth: Recruiter (creator or Admin)
Response: 204
```

#### `PATCH /api/jobs/{jobId}/publish` — Toggle publish status
```
Auth: Recruiter
Body: { "isPublished": boolean }
Response: 200
```

#### `GET /api/jobs/recruiter` — My company's jobs (recruiter view)
```
Auth: Recruiter
Query: isPublished?, page, pageSize
Response: 200 → PagedResult<JobListDto>
```

#### `POST /api/jobs/{jobId}/apply` — Apply for a job
```
Auth: Candidate
Body: ApplyJobRequest
Response: 201 → ApplyJobResponse
Errors: 400 (no resume / already applied) | 404 (job not found)
```
> ⚠️ `matchScore` will be `null` initially — AI scoring runs asynchronously. If AI is down, application still succeeds but score stays `null` until retry.

#### `GET /api/jobs/{jobId}/applicants` — View applicants (recruiter)
```
Auth: Recruiter (of this job's company)
Query: ApplicantSearchParams (status?, minScore?, sortBy, sortOrder, page, pageSize)
Response: 200 → PagedResult<ApplicantDto>
```

#### `PATCH /api/jobs/{jobId}/applicants/{applicationId}/status` — Change application status
```
Auth: Recruiter
Body: UpdateStatusRequest
Response: 200
Errors: 400 (invalid transition — see pipeline rules below)
```

#### `PATCH /api/jobs/{jobId}/applicants/{applicationId}/rating` — Rate applicant
```
Auth: Recruiter
Body: UpdateRatingRequest
Response: 200
```

#### `GET /api/jobs/{jobId}/applicants/export` — Export applicants as CSV
```
Auth: Recruiter
Response: 200 (Content-Type: text/csv)
Headers: Content-Disposition: attachment; filename="applicants.csv"
Columns: Candidate Name, Email, Job Title, Applied Date, Status, Rating, Match Score
```
**Frontend handling:**
```typescript
const response = await api.get(`/jobs/${jobId}/applicants/export`, {
  responseType: 'blob',
});
const url = window.URL.createObjectURL(new Blob([response.data]));
const link = document.createElement('a');
link.href = url;
link.download = 'applicants.csv';
link.click();
```

---

### 🤖 AI — `/api/ai`

> ⚠️ All AI endpoints may return **503 Service Unavailable** if the Python AI microservice is down. Show a user-friendly message like *"AI service is temporarily unavailable. Please try again later."*

> **Resilience**: Backend retries 3 times with exponential backoff (2s → 4s → 8s). Timeout: 45 seconds per request.

#### `POST /api/ai/extract-skills`
```
Auth: Recruiter
Body: ExtractSkillsRequest
Response: 200 → ExtractSkillsResponse
```

#### `POST /api/ai/analyze-resume`
```
Auth: Recruiter (must own the job)
Body: AnalyzeResumeRequest
Response: 200 → AnalyzeResumeResponse
```

#### `POST /api/ai/generate-assessment`
```
Auth: Recruiter
Body: GenerateAssessmentRequest
Response: 200 → GenerateAssessmentResponse
```

#### `POST /api/ai/generate-cv`
```
Auth: Candidate (own resume only)
Body: GenerateCvRequest
Response: 200 → GenerateCvResponse
```

#### `POST /api/ai/interview-questions`
```
Auth: Recruiter
Body: GenerateInterviewQuestionsRequest
Response: 200 → GenerateInterviewQuestionsResponse
```

#### `POST /api/ai/score-interview` *(system-internal, may also be called by recruiter)*
```
Auth: System / Recruiter
Body: { interviewId, transcript, questions }
Response: 200 → { score, feedback, detailedScores: [{ question, score, notes }] }
```

---

### 📋 Assessments — `/api/assessments`

#### `POST /api/assessments` — Create assessment with questions
```
Auth: Recruiter (must own the job or be Admin)
Body: CreateAssessmentRequest
Response: 201 → AssessmentDetailDto
```
> `totalScore` is auto-calculated from the sum of question `points`.

#### `GET /api/assessments/{assessmentId}` — Get assessment details
```
Auth: Recruiter (job owner) OR Candidate (at Assessment stage)
Response: 200 → AssessmentDetailDto
```
> ⚠️ **`correctAnswer` is HIDDEN from candidates** — only visible to recruiters.

#### `PUT /api/assessments/{assessmentId}` — Update assessment
```
Auth: Recruiter (job owner or Admin)
Body: CreateAssessmentRequest (all fields optional for partial update)
Response: 200 → AssessmentDetailDto
```

#### `DELETE /api/assessments/{assessmentId}` — Delete assessment
```
Auth: Recruiter (job owner or Admin)
Response: 204
```

#### `GET /api/assessments/job/{jobPostId}` — List assessments for a job
```
Auth: Recruiter (of the job's company)
Response: 200 → AssessmentListDto[]
```

#### `POST /api/assessments/{assessmentId}/start` — Start assessment attempt
```
Auth: Candidate (must be at Assessment stage)
Response: 200 → StartAssessmentResponse
Errors: 400 (already attempted) | 403 (not at assessment stage)
```
> ⚠️ **CRITICAL**: The response includes `deadlineAt`. You **must** build a countdown timer and submit before this time.

#### `POST /api/assessments/{assessmentId}/submit` — Submit answers
```
Auth: Candidate (must have started attempt)
Body: SubmitAssessmentRequest
Response: 200 → SubmitAssessmentResponse
Errors: 400 ("Time window expired" — submitted after deadlineAt)
```

#### `GET /api/assessments/{assessmentId}/results` — View all candidate scores
```
Auth: Recruiter (of the job's company)
Query: page, pageSize
Response: 200 → PagedResult<CandidateAssessmentResultDto>
```

---

### 📅 Interviews — `/api/interviews`

#### `POST /api/interviews` — Schedule interview
```
Auth: Recruiter
Body: ScheduleInterviewRequest
Response: 201 → InterviewDetailDto
Precondition: Application must be at "Interview" status
```
> For Live interviews, `meetingLink` is auto-generated by the backend. Notifications are sent to both candidate and recruiter.

#### `GET /api/interviews/{interviewId}` — Get interview details
```
Auth: Recruiter or Candidate (participant)
Response: 200 → InterviewDetailDto
```

#### `GET /api/interviews` — List interviews
```
Auth: Recruiter or Candidate
Query: InterviewSearchParams (status?, interviewType?, upcoming?, page, pageSize)
Response: 200 → PagedResult<InterviewListDto>
```

#### `POST /api/interviews/{interviewId}/start-ai` — Start AI interview
```
Auth: Candidate
Response: 200 → StartAiInterviewResponse
Precondition: Current time must be within 15 minutes before scheduledAt
```

#### `POST /api/interviews/{interviewId}/complete-ai` — Submit AI interview transcript
```
Auth: Candidate
Body: CompleteAiInterviewRequest
Response: 200 → CompleteAiInterviewResponse
```

#### `POST /api/interviews/{interviewId}/join` — Join live interview
```
Auth: Candidate or Recruiter (participant)
Response: 200 → JoinLiveInterviewResponse
Precondition: Current time must be within 15 minutes before scheduledAt
```

#### `PATCH /api/interviews/{interviewId}/complete` — Complete live interview
```
Auth: Recruiter
Body: CompleteLiveInterviewRequest
Response: 200 → InterviewDetailDto
```

#### `PATCH /api/interviews/{interviewId}/cancel` — Cancel interview
```
Auth: Recruiter
Body: CancelInterviewRequest
Response: 200 → { "interviewId": number, "status": "Cancelled" }
```

---

### 🔔 Notifications — `/api/notifications`

#### `GET /api/notifications` — Get notification history
```
Auth: Any authenticated user
Query: isRead? (boolean), page, pageSize (default: 20)
Response: 200 → PagedResult<NotificationDto>
```

#### `GET /api/notifications/unread-count`
```
Auth: Any authenticated user
Response: 200 → { "count": number }
```

#### `PATCH /api/notifications/{notificationId}/read`
```
Auth: Any authenticated user
Response: 200 → { "id": number, "isRead": true }
```

#### `PATCH /api/notifications/read-all`
```
Auth: Any authenticated user
Response: 200 → { "updatedCount": number }
```

### Notification Types & Triggers

| Event | Recipients | Channels |
|-------|-----------|----------|
| Application submitted | Recruiter(s) of the job | In-app + Email |
| Application status changed | Candidate | In-app + Email |
| Interview scheduled | Candidate + Recruiter | In-app + Email |
| Interview reminder (24h + 1h before) | Candidate + Recruiter | Email only |
| Assessment assigned | Candidate | In-app + Email |
| Candidate accepted | Candidate | In-app + Email |
| Candidate rejected | Candidate | In-app + Email |
| AI score ready | Recruiter(s) | In-app only |

---

## 📡 8. Real-Time Features (SignalR)

### Setup

Install the SignalR client:
```bash
npm install @microsoft/signalr
```

### Notifications Hub

```typescript
import * as signalR from '@microsoft/signalr';

const notificationConnection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7001/hubs/notifications', {
    accessTokenFactory: () => localStorage.getItem('ies_token') || '',
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .configureLogging(signalR.LogLevel.Warning)
  .build();

// ── Listen for new notifications ──
notificationConnection.on('ReceiveNotification', (notification: NotificationDto) => {
  // Show toast / add to notification list
  console.log('New notification:', notification);
});

// ── Listen for unread count updates ──
notificationConnection.on('UpdateUnreadCount', (count: number) => {
  // Update badge in navbar
  console.log('Unread count:', count);
});

// ── Connect on login ──
async function connectNotifications() {
  try {
    await notificationConnection.start();
    console.log('Connected to notifications hub');
  } catch (err) {
    console.error('Failed to connect:', err);
    setTimeout(connectNotifications, 5000);
  }
}

// ── Disconnect on logout ──
async function disconnectNotifications() {
  await notificationConnection.stop();
}
```

> User is automatically joined to a personal group (keyed by userId). Notifications are stored in DB if the user is offline — they'll see them when they next fetch `/api/notifications`.

### Interview Hub (WebRTC Signaling)

```typescript
const interviewConnection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7001/hubs/interview', {
    accessTokenFactory: () => localStorage.getItem('ies_token') || '',
  })
  .withAutomaticReconnect()
  .build();

// ── Client sends to server ──
await interviewConnection.invoke('JoinRoom', roomId);
await interviewConnection.invoke('SendOffer', roomId, sdpOffer);
await interviewConnection.invoke('SendAnswer', roomId, sdpAnswer);
await interviewConnection.invoke('SendIceCandidate', roomId, iceCandidate);
await interviewConnection.invoke('LeaveRoom', roomId);

// ── Server sends to client ──
interviewConnection.on('UserJoined', (userId: string) => { /* peer joined */ });
interviewConnection.on('UserLeft', (userId: string) => { /* peer left */ });
interviewConnection.on('ReceiveOffer', (sdpOffer: string) => { /* create answer */ });
interviewConnection.on('ReceiveAnswer', (sdpAnswer: string) => { /* set remote desc */ });
interviewConnection.on('ReceiveIceCandidate', (candidate: string) => { /* add ice */ });
```

### WebRTC Live Interview Flow

```
1. Call POST /api/interviews/{id}/join → get { roomId, signalingHubUrl }
2. Connect to SignalR interview hub
3. Invoke JoinRoom(roomId)
4. Request camera/mic: navigator.mediaDevices.getUserMedia({ video: true, audio: true })
5. Create RTCPeerConnection
6. Add local stream tracks to peer connection
7. CreateOffer → setLocalDescription → invoke SendOffer(roomId, sdp)
8. Listen for ReceiveAnswer → setRemoteDescription
9. Exchange ICE candidates via SendIceCandidate / ReceiveIceCandidate
10. On call end: recruiter calls PATCH /api/interviews/{id}/complete
```

---

## 📁 9. File Uploads Guide

### General Rules

| File Type | Accepted Formats | Max Size | Endpoint |
|-----------|-----------------|----------|----------|
| Resume | `.pdf`, `.docx` | 10 MB | `POST /api/candidates/resume` |
| Profile Picture | `.jpg`, `.png` | 5 MB | `PUT /api/candidates/profile-picture` |
| Company Logo | `.jpg`, `.png` | 5 MB | `PUT /api/companies/{id}/logo` |

### Upload Code Example

```typescript
async function uploadResume(file: File, isDefault: boolean = false) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('isDefault', String(isDefault));

  const response = await api.post('/candidates/resume', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    onUploadProgress: (progressEvent) => {
      const percent = Math.round((progressEvent.loaded * 100) / (progressEvent.total || 1));
      console.log(`Upload progress: ${percent}%`);
    },
  });

  return response.data as ResumeSummaryDto;
}

async function uploadProfilePicture(file: File) {
  const formData = new FormData();
  formData.append('image', file);

  const response = await api.put('/candidates/profile-picture', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });

  return response.data.profilePicturePath;
}
```

### Client-Side Validation (Before Upload)

```typescript
function validateResume(file: File): string | null {
  const allowedTypes = [
    'application/pdf',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  ];
  if (!allowedTypes.includes(file.type)) return 'Only PDF and DOCX files are allowed';
  if (file.size > 10 * 1024 * 1024) return 'File size must be under 10MB';
  return null;
}

function validateImage(file: File): string | null {
  const allowedTypes = ['image/jpeg', 'image/png'];
  if (!allowedTypes.includes(file.type)) return 'Only JPG and PNG files are allowed';
  if (file.size > 5 * 1024 * 1024) return 'File size must be under 5MB';
  return null;
}
```

---

## 🚦 10. Business Rules & Edge Cases

### 1. Strict Hiring Pipeline (CRITICAL ⚠️)

Application statuses **must** follow this exact sequence. The backend enforces this — invalid transitions return `400 Bad Request`.

```
Pending → UnderReview → Assessment → Interview → Accepted
              ↓              ↓           ↓
           Rejected       Rejected    Rejected
```

**Rules:**
- ❌ Cannot skip stages (e.g., Pending → Interview)
- ❌ Cannot move backwards (e.g., Interview → UnderReview)
- ✅ Can reject from **any** active stage
- ✅ `Accepted` and `Rejected` are terminal states — no further transitions

**Frontend implementation:** Build a pipeline/kanban UI showing these stages. Disable forward buttons that skip stages.

### 2. Resume Required for Applications

A candidate **cannot** apply unless they have uploaded at least one resume. The backend returns `400 Bad Request` if `resumeId` is missing or invalid.

**Frontend:** Check `profile.resumes.length > 0` before showing the "Apply" button. Show a prompt to upload a resume first.

### 3. AI Graceful Degradation

When calling AI endpoints:
- The Python AI microservice may return **503 Service Unavailable**
- **For job applications:** If AI scoring fails, the application **still succeeds** (`201 Created`) but `matchScore` is `null`. The backend retries automatically in the background.
- **Show:** A "Score Pending" badge/spinner instead of a score value
- **For direct AI calls** (extract skills, generate assessment, etc.): Show a user-friendly error toast

### 4. Immutable Data

- `taxNumber` (Company) — cannot be changed after creation. Hide/disable this field in edit forms.
- Passwords are never returned in any response.

### 5. Assessment Time Limits

When a candidate starts an assessment:
1. Backend returns `deadlineAt` (ISO 8601 timestamp)
2. **MUST** build a visible countdown timer
3. **MUST** auto-submit when timer reaches zero
4. Submitting after `deadlineAt` → `400 Bad Request` ("Time window expired")

```typescript
// Calculate remaining time
const deadline = new Date(startResponse.deadlineAt);
const remaining = deadline.getTime() - Date.now();

// Auto-submit when time expires
const timer = setTimeout(() => {
  submitAssessment(answers);
}, remaining);
```

### 6. Interview Time Windows

- Participants can only **join** a live interview or **start** an AI interview within **15 minutes before** the `scheduledAt` time.
- Attempting to join earlier returns `400 Bad Request`.
- **Frontend:** Show a "Join" button that becomes active 15 minutes before `scheduledAt`. Show a countdown until the button activates.

### 7. Duplicate Application Prevention

- A candidate can apply to each job **only once** (unique constraint on `CandidateId + JobPostId`)
- Backend returns `400` if already applied
- **Frontend:** Check application status before showing "Apply" button. Show "Already Applied" with current status instead.

### 8. Saved Jobs Toggle

`POST /api/candidates/saved-jobs/{jobPostId}` is a **toggle**:
- If job is not saved → saves it, returns `{ "isSaved": true }`
- If job is already saved → unsaves it, returns `{ "isSaved": false }`

### 9. Dates and Currency

- **All dates** must be in **ISO 8601** format: `2026-03-05T14:30:00Z`
- **Currency** defaults to Egyptian Pounds (`EGP`) unless specifically set
- Display salary ranges as: `EGP 15,000 – 25,000` (or use the `currency` field from the job)

### 10. CSV Export (Binary Download)

The applicants export endpoint returns raw CSV bytes. Handle as blob download (see example in Jobs section above).

---

## 🔄 11. State Machines

### Application Status Pipeline

```
┌─────────┐    ┌──────────────┐    ┌────────────┐    ┌───────────┐    ┌──────────┐
│ Pending  │───→│ UnderReview  │───→│ Assessment │───→│ Interview │───→│ Accepted │
└────┬─────┘    └──────┬───────┘    └─────┬──────┘    └─────┬─────┘    └──────────┘
     │                 │                  │                 │           (Terminal)
     │                 ▼                  ▼                 ▼
     │           ┌──────────┐       ┌──────────┐      ┌──────────┐
     └──────────→│ Rejected │←──────│ Rejected │←─────│ Rejected │
                 └──────────┘       └──────────┘      └──────────┘
                   (Terminal)
```

### Interview Status Flow

```
┌───────────┐    ┌─────────────┐    ┌───────────┐
│ Scheduled │───→│ InProgress  │───→│ Completed │
└─────┬─────┘    └─────────────┘    └───────────┘
      │                              (Terminal)
      ▼
┌───────────┐
│ Cancelled │
└───────────┘
  (Terminal)
```

---

## ❌ 12. Error Handling Reference

### HTTP Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST (new resource) |
| 204 | No Content | Successful DELETE or logout |
| 400 | Bad Request | Validation error, invalid transition, expired timer |
| 401 | Unauthorized | Missing/invalid/expired/blacklisted token |
| 403 | Forbidden | Authenticated but not authorized (wrong role) |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate (email, taxNumber) |
| 503 | Service Unavailable | AI microservice down |

### Error Response Format

All errors follow this standardized envelope:

```json
{
  "statusCode": 400,
  "message": "Invalid status transition. Cannot skip from Pending to Interview.",
  "details": null
}
```

### Recommended Error Handling Strategy

```typescript
function handleApiError(error: ApiErrorResponse) {
  switch (error.statusCode) {
    case 400:
      // Show validation message to user
      showToast(error.message, 'warning');
      break;
    case 401:
      // Redirect to login
      clearAuth();
      router.navigate('/login');
      break;
    case 403:
      // Show "Access Denied" page
      showToast('You do not have permission for this action', 'error');
      break;
    case 404:
      // Show "Not Found" page
      router.navigate('/404');
      break;
    case 409:
      // Show conflict message (e.g., email already taken)
      showToast(error.message, 'error');
      break;
    case 503:
      // AI service down — graceful degradation
      showToast('AI service is temporarily unavailable. Please try again later.', 'info');
      break;
    default:
      showToast('An unexpected error occurred', 'error');
  }
}
```

---

> **Questions?** Reach out to the backend team. All endpoints are testable via Swagger UI at `https://localhost:7001/`.

*Document generated from backend specifications and API contracts — v2.0*

# Feature Specification: IES Backend Implementation

**Feature Branch**: `001-ies-backend-implementation`  
**Created**: 2026-03-04  
**Status**: Draft  
**Input**: User description: "Implement the complete backend for the Intelligent Employment System — a graduation project for an AI-powered recruitment platform. The backend covers authentication, user management, company & job management, AI-powered resume scoring, assessments, interviews (AI + live), and notifications."

## Clarifications

### Session 2026-03-04

- Q: How does the first recruiter for a new company get onboarded (chicken-and-egg with CompanyId requirement)? → A: ~~A platform super-admin pre-creates companies; recruiters then join using a CompanyId or invite code.~~ **REVISED in Session 2 below.**
- Q: Is a resume required before a candidate can apply for a job? → A: Yes — resume is mandatory; no resume means no application allowed.
- Q: Can a recruiter skip pipeline stages (e.g., jump from Pending to Interview)? → A: No — strict sequential; every candidate must pass through every stage in order.
- Q: How should session expiry and renewal work? → A: Single long-lived token (e.g., 24 hours); no refresh token mechanism. Simple approach suitable for a graduation project.
- Q: How do "similar jobs" and "AI-suggested jobs" differ — do both require AI? → A: Similar jobs use simple rule-based matching (shared skills, same career level, same industry) with no AI dependency. AI-suggested jobs use the AI service to match against the candidate's full profile.

### Session 2026-03-12 — Design Review (25 Decisions)

**Structural:**
- Q1: No separate Super Admin entity. The first recruiter who registers a company automatically becomes Admin Recruiter.
- Q2: Recruiters join existing companies via a 6-character alphanumeric Invite Code (max 5 uses + expiry date). Admin Recruiter generates codes.
- Q3: Junior Recruiter role removed — only Admin and Standard remain. Unjustified complexity for MVP.
- Q4: Admin transfer endpoint added: `PUT /companies/{id}/transfer-admin`. Old admin becomes Standard.

**Data & Requirements:**
- Q5: Multiple resumes allowed with `IsDefault` flag. Applications use the default resume unless specified.
- Q6: Match Score is three-state: `null` = not yet calculated (display "Processing", sort to bottom), `0.0` = calculated zero, `0.0–100.0` = normal.
- Q7: `IsVerified` field removed from Company. Not needed in MVP — all companies are treated equally.
- Q8: No limit on job posts per company in MVP.
- Q9: Expired jobs: hidden from search via query-time filtering + background service sets `IsActive = false` every 24h. Existing applicants still see them. Recruiter can extend ExpiryDate.

**Permissions:**
- Q10: Standard Recruiter can view all company jobs but can only edit their own.
- Q11: Candidates can withdraw applications (status = `Withdrawn`) only while `Pending`. Withdrawal is final — no re-application allowed.
- Q12: Recruiters cannot browse candidate profiles outside their job applicants. Not in MVP.
- Q13: Match Score is computed once at application time. Profile edits do not trigger recalculation.

**Technical:**
- Q14: AI service is a Python microservice called via HTTP. C# AiServiceClient falls back to stub values if unavailable.
- Q15: AI Voice Interview replaced with text-based AI interview. Candidate answers written questions, AI scores responses.
- Q16: Live interviews use third-party WebRTC provider (Daily.co or 100ms free tier). Backend only creates rooms and returns links.
- Q17: Email: console logging in dev, Mailtrap for demo, SendGrid free tier for production.

**Improvements:**
- Q18: Candidate invitation by recruiter — future work, not in MVP.
- Q19: Messaging system — future work, not in MVP.
- Q20: Skill Level added to CandidateSkill junction (Beginner=1, Intermediate=2, Expert=3). Also `RequiredLevel` on JobPostSkill.
- Q21: CandidateEducation (Degree, FieldOfStudy, Institution, GraduationYear) and CandidateExperience (JobTitle, Company, Description, StartDate, EndDate?) added.
- Q22: Soft Delete (DeletedAt + DeletedBy nullable) applied to Jobs and Applications only.

**Scope:**
- Q23: MVP = Phases 1–11. Phases 12–14 = future work / frontend aggregation.
- Q24: Tests: Unit Tests for services (xUnit + Moq) + Integration Tests for Auth endpoints (WebApplicationFactory).
- Q25: Dashboards deferred to minimal — aggregate numbers only, no new backend endpoints needed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — User Registration & Authentication (Priority: P1)

A new user (candidate or recruiter) visits the platform and creates an account. Candidates register via `POST /api/auth/register` with personal details. Recruiters have two paths: (A) create a new company during registration via `POST /api/auth/register/company`, automatically becoming Admin Recruiter, or (B) join an existing company using a 6-character invite code via `POST /api/auth/register/recruiter`, receiving Standard role. All users can log in, receive a long-lived session token (24 hours) with their role embedded, and are routed to the appropriate dashboard. Users can log out and reset their passwords.

**Why this priority**: Authentication is the foundational gate — no other feature works without it. Every user journey begins with account creation or login.

**Independent Test**: Can be fully tested by creating a candidate account, creating a recruiter + company, generating an invite code, joining with another recruiter, logging in, and verifying tokens contain correct roles.

**Acceptance Scenarios**:

1. **Given** a new candidate, **When** they submit valid registration details via `POST /api/auth/register`, **Then** an account is created and the response includes a JWT token with role "Candidate".
2. **Given** a new recruiter, **When** they submit registration details with company info (name, taxNumber, industry) via `POST /api/auth/register/company`, **Then** a company is created, a recruiter account is created with Admin role, and a JWT token is returned.
3. **Given** an existing company with a valid invite code, **When** a new recruiter submits registration details with the invite code via `POST /api/auth/register/recruiter`, **Then** a recruiter account is created with Standard role under that company.
4. **Given** valid credentials, **When** a user logs in, **Then** the response includes a secure session token and a role field (Candidate / Recruiter / Admin).
5. **Given** invalid credentials, **When** a user attempts to log in, **Then** the system returns an authentication error without revealing which field is incorrect.
6. **Given** an authenticated user, **When** they log out, **Then** the session token is invalidated.

---

### User Story 2 — Candidate Profile & Resume Management (Priority: P1)

A candidate completes their profile by adding a job title, professional summary, years of experience, skills (with proficiency level: Beginner/Intermediate/Expert), education history (degree, field of study, institution, graduation year), and work experience (job title, company, description, dates). They upload their resume (PDF/DOCX), and the system stores it. The candidate can also request an AI-generated CV based on their profile data. The candidate can view and update their profile at any time.

**Why this priority**: The candidate profile and resume are prerequisites for job applications, AI scoring, and all downstream candidate features.

**Independent Test**: Can be tested by creating a candidate, updating their profile, adding education and experience entries, setting skills with levels, uploading a resume file, and verifying all data is persisted and retrievable.

**Acceptance Scenarios**:

1. **Given** an authenticated candidate, **When** they update their profile with job title, summary, and experience years, **Then** the profile data is saved and retrievable.
2. **Given** an authenticated candidate, **When** they add skills with proficiency levels (Beginner/Intermediate/Expert) to their profile, **Then** the skills and their levels are associated with the candidate record.
3. **Given** an authenticated candidate, **When** they add education entries (degree, field of study, institution, graduation year), **Then** the education data is saved and included in their profile.
4. **Given** an authenticated candidate, **When** they add work experience entries (job title, company, description, start date, end date), **Then** the experience data is saved. A null end date indicates current employment.
5. **Given** an authenticated candidate, **When** they upload a PDF or DOCX resume, **Then** the file is stored and linked to their profile.
6. **Given** an authenticated candidate with a complete profile, **When** they request AI CV generation, **Then** the system produces an AI-enhanced resume and stores its path.
7. **Given** an invalid file type, **When** a candidate attempts to upload, **Then** the system rejects the upload with a clear error message.

---

### User Story 3 — Company Registration & Management (Priority: P1)

Companies are created during recruiter registration (the first recruiter who registers with company details becomes Admin Recruiter automatically). The Admin Recruiter can edit company information, generate invite codes (6-character alphanumeric, max 5 uses, with expiry), and transfer the admin role to another recruiter. Standard Recruiters join via invite codes. Anyone can view company information and job listings publicly.

**Why this priority**: Companies are the organizational unit for recruiters and jobs — without companies, recruiters cannot post jobs or manage applications.

**Independent Test**: Can be tested by creating a company (via recruiter registration), generating invite codes, having another recruiter join, editing company details, transferring admin, and verifying public company data is accessible.

**Acceptance Scenarios**:

1. **Given** a recruiter registering with company details, **When** the registration succeeds, **Then** the company is created and the recruiter becomes Admin.
2. **Given** an Admin Recruiter, **When** they generate an invite code, **Then** the code is created with a usage limit (default 5) and expiry date.
3. **Given** a valid invite code, **When** a new recruiter registers with it, **Then** the recruiter joins the company as Standard and the code's usage count increments.
4. **Given** a duplicate tax number, **When** a company registration is attempted, **Then** the system rejects it with a uniqueness error.
5. **Given** any user, **When** they request company details, **Then** public company information and job listings are returned.
6. **Given** an Admin Recruiter, **When** they transfer admin to another recruiter in the same company, **Then** the target becomes Admin and the original becomes Standard.
7. **Given** an expired or fully-used invite code, **When** a recruiter attempts to register with it, **Then** the system rejects the registration.

---

### User Story 4 — Job Posting & Management (Priority: P1)

A recruiter creates a job post through a structured flow — providing title, description, required skills, salary range, career level, and other details. The job can be saved as a draft (unpublished) or published immediately. Recruiters can edit, delete, and publish/unpublish their job posts. All users can search and browse published job listings with filters.

**Why this priority**: Job posts are the core content of the platform — they connect candidates to opportunities and drive all application, scoring, and interview workflows.

**Independent Test**: Can be tested by creating a job post as draft, publishing it, searching for it with filters, and verifying it appears in public listings.

**Acceptance Scenarios**:

1. **Given** a recruiter, **When** they create a job post with all required fields, **Then** the job is saved. If marked as draft, the job remains unpublished.
2. **Given** a draft job, **When** a recruiter publishes it, **Then** the job becomes active and appears in public listings.
3. **Given** published jobs exist, **When** any user searches with filters (location, job type, career level, industry, salary range), **Then** matching jobs are returned with pagination.
4. **Given** a job post, **When** its details are retrieved, **Then** the response includes an applicants count.
5. **Given** a published job, **When** a candidate views its details, **Then** similar jobs are shown based on simple rule-based matching (shared skills, same career level, same industry) — no AI service required.

---

### User Story 5 — Job Application & Tracking (Priority: P2)

A candidate finds a job they are interested in and applies. The system records the application with a "Pending" status. Upon application, the system triggers AI-based resume analysis to calculate an ATS match score. The candidate can track the status of all their applications. Candidates can also save/bookmark jobs for later.

**Why this priority**: Applications are the primary transaction of the platform — connecting candidates to jobs and triggering the AI scoring pipeline.

**Independent Test**: Can be tested by applying for a job, verifying the application is recorded, checking that ATS scoring is triggered, and tracking application status changes.

**Acceptance Scenarios**:

1. **Given** an authenticated candidate with an uploaded resume, **When** they apply for a published job, **Then** an application is created with status "Pending" and the application date is recorded.
1b. **Given** an authenticated candidate without an uploaded resume, **When** they attempt to apply, **Then** the system rejects the application and instructs them to upload a resume first.
2. **Given** a new application, **When** the application is submitted, **Then** the AI service is called to calculate a match score, and the score is stored with the application.
3. **Given** a candidate with applications, **When** they view their applications list, **Then** all applications are returned with current status, job details, and match score.
4. **Given** a candidate, **When** they save a job, **Then** the job is bookmarked. Calling save again toggles it off.
5. **Given** a candidate has already applied for a job, **When** they attempt to apply again, **Then** the system prevents duplicate applications.
6. **Given** a candidate with a Pending application, **When** they withdraw the application, **Then** the status changes to "Withdrawn" (terminal state). Re-application to the same job is not allowed.

---

### User Story 6 — Applicant Management & Screening (Priority: P2)

A recruiter views the list of applicants for a job, sorted by AI match score. They can filter applicants by status and experience level. The recruiter can rate applicants (1-5 stars), change application status (move through the pipeline), and export the applicant list as a CSV file. The recruiter can view individual candidate profiles and resumes.

**Why this priority**: This is the recruiter's primary workflow for candidate evaluation — without it, hiring decisions cannot be made.

**Independent Test**: Can be tested by viewing applicants for a job, setting ratings, filtering by status, and exporting to CSV.

**Acceptance Scenarios**:

1. **Given** a job with applicants, **When** a recruiter views the applicants list, **Then** applicants are returned with name, email, match score, status, rating, and applied date — paginated.
2. **Given** an applicant, **When** a recruiter sets a star rating (1-5), **Then** the rating is saved and reflected in subsequent queries.
3. **Given** applicants with various statuses, **When** a recruiter filters by status or experience, **Then** only matching applicants are returned.
4. **Given** a job with applicants, **When** a recruiter exports to CSV, **Then** a CSV file is generated containing candidate name, email, job applied for, applied date, status, rating, and match score.

---

### User Story 7 — AI Resume Scoring & Skill Extraction (Priority: P2)

When a candidate applies for a job, the system sends the candidate's resume and the job requirements to the AI service. The AI service analyzes the resume for relevance — matching skills, experience, and context — and returns a match score (0-100) along with a skills gap analysis. When a recruiter creates a job, they can use an AI assistant to extract structured requirements from a free-text job description.

**Why this priority**: AI scoring is the key differentiator of this platform — it replaces manual keyword-matching with intelligent, context-aware candidate evaluation.

**Independent Test**: Can be tested by submitting a resume and job description to the AI endpoints and verifying a score and skills analysis are returned.

**Acceptance Scenarios**:

1. **Given** a candidate resume and job requirements, **When** the AI resume analysis is triggered, **Then** a match score (0-100) and a match report are returned.
2. **Given** a free-text job description, **When** the AI skill extraction endpoint is called, **Then** structured skills with context are returned.
3. **Given** a recruiter composing a job post, **When** they use the AI assistant, **Then** the system extracts required skills, level, industry, and personality traits from the description.
4. **Given** the AI service is unavailable, **When** an application is submitted, **Then** the application is still recorded with the match score marked as pending, and scoring is retried later.

---

### User Story 8 — Assessment Creation & Evaluation (Priority: P3)

A recruiter creates an assessment for a specific job, either manually or by requesting AI-generated questions from the job description. The assessment contains questions of various types (MCQ, True/False, Open-Ended, Coding). Candidates who reach the assessment stage are invited to complete the assessment within a time window. Their answers are scored, and results are available to the recruiter.

**Why this priority**: Assessments add a structured evaluation layer beyond resume scoring, but they depend on jobs and applications being functional first.

**Independent Test**: Can be tested by creating an assessment with various question types, having a candidate submit answers, and verifying scoring results.

**Acceptance Scenarios**:

1. **Given** a recruiter with an active job, **When** they create an assessment with questions, **Then** the assessment is linked to the job with a defined time window and total score.
2. **Given** a job description, **When** a recruiter requests AI-generated assessment, **Then** the system generates relevant questions based on the description.
3. **Given** a candidate at the assessment stage, **When** they submit answers within the time window, **Then** answers are scored and results are stored.
4. **Given** the assessment time window has expired, **When** a candidate attempts to submit, **Then** the submission is rejected.

---

### User Story 9 — Interview Scheduling & Conduct (Priority: P3)

A recruiter schedules interviews for shortlisted candidates — either a text-based AI interview or a live video interview. For AI interviews, the system generates custom questions based on the job description; the candidate answers in writing and the AI scores the responses. For live interviews, the backend creates a room via a third-party WebRTC provider (Daily.co / 100ms) and returns a meeting link to both parties.

**Why this priority**: Interviews are the final evaluation step before hiring decisions — they depend on all prior pipeline stages being operational.

**Independent Test**: Can be tested by scheduling both types of interviews, completing a text-based AI interview, and verifying scores and feedback are recorded.

**Acceptance Scenarios**:

1. **Given** a shortlisted candidate, **When** a recruiter schedules an AI interview, **Then** the interview is created with type "AI", a scheduled time, and status "Scheduled".
2. **Given** a scheduled AI interview, **When** the candidate starts it, **Then** custom text-based questions are generated and returned to the candidate.
3. **Given** AI interview questions, **When** the candidate submits written answers, **Then** the AI scores the responses and returns a score with feedback.
4. **Given** a shortlisted candidate, **When** a recruiter schedules a live interview, **Then** a meeting room is created via third-party provider and a link is returned.
5. **Given** a completed interview, **When** the recruiter ends it, **Then** feedback notes and an interview score are recorded.

---

### User Story 10 — Notifications & Candidate Acceptance (Priority: P3)

The system sends email notifications at key points in the hiring pipeline: interview reminders, application status changes, and acceptance/rejection notifications. When a recruiter accepts a candidate, the system triggers an acceptance email with next steps. Real-time in-app notifications are also delivered when users are online.

**Why this priority**: Notifications are a communication layer that enhances user experience but depends on all pipeline stages being in place.

**Independent Test**: Can be tested by triggering each notification event and verifying emails are sent and in-app notifications are delivered.

**Acceptance Scenarios**:

1. **Given** an interview is scheduled, **When** the scheduled time approaches, **Then** reminder emails are sent to both candidate and recruiter.
2. **Given** an application status changes, **When** a recruiter moves an applicant to a new stage, **Then** the candidate receives a notification.
3. **Given** a candidate is accepted, **When** the recruiter confirms acceptance, **Then** an acceptance email is sent to the candidate.
4. **Given** a notification event occurs, **When** the recipient is online, **Then** a real-time in-app notification is delivered.

---

### User Story 11 — Dashboards & Analytics (Priority: P3)

Candidates see a dashboard with application count, profile views, saved jobs count, recent activity, and upcoming interviews. Recruiters see a company dashboard with total applicants, active jobs, hire rate, application trends (weekly chart data), recent job posts, and action-required items. Both dashboards provide at-a-glance summaries to guide user actions.

**Why this priority**: Dashboards aggregate data from all other features — they are a presentation concern that adds polish but doesn't block core workflows.

**Independent Test**: Can be tested by calling dashboard endpoints and verifying aggregated metrics match the underlying data.

**Acceptance Scenarios**:

1. **Given** a candidate with activity, **When** they view their dashboard, **Then** applications count, saved jobs count, upcoming interviews, and recent activity are returned.
2. **Given** a company with jobs and applicants, **When** a recruiter views the company dashboard, **Then** totalApplicants, activeJobs, hireRate, applicationTrends, and actionRequired items are returned.
3. **Given** the AI service is available, **When** a candidate views their dashboard, **Then** AI-suggested jobs matched against the candidate's full profile (skills, experience, summary) are included. If the AI service is unavailable, this section is omitted gracefully.

---

### Edge Cases

- What happens when a candidate applies without uploading a resume? — The system rejects the application and prompts the candidate to upload a resume first. A resume is mandatory for all applications.
- How does the system handle concurrent applications to the same job by the same candidate? — The system must enforce one application per candidate per job.
- What happens when the AI microservice is down during resume scoring? — Applications should still be accepted; scoring should be queued and retried.
- What happens when a recruiter tries to add a recruiter to a company they don't own? — The system must verify the admin recruiter belongs to the same company.
- How are expired job posts handled? — Jobs past their ExpiryDate should be automatically marked inactive or filtered from public listings.
- What happens when a candidate tries to join an interview before the scheduled time? — The system should enforce a time window (e.g., 15 minutes before).
- How does the system handle large file uploads? — File size limits must be enforced (e.g., 10MB for resumes), with clear error messages.
- What happens when a company's tax number is changed? — Tax numbers should be immutable after registration to maintain verification integrity.
- What happens when a recruiter tries to skip a pipeline stage or move a candidate backwards? — The system must reject the transition and enforce the strict sequential order: Pending → UnderReview → Assessment → Interview → Accepted/Rejected. Rejection is allowed from any stage.
- What happens when a candidate withdraws an application? — Withdrawal is only allowed when status is Pending. The status becomes Withdrawn (terminal). Re-application to the same job is not permitted.
- What happens when an invite code expires or reaches its usage limit? — Registration with that code is rejected. The Admin Recruiter can generate a new code.
- What happens when the only Admin Recruiter leaves the company? — The system provides a transfer-admin endpoint. The old Admin becomes Standard after transfer.
- What happens when a job is soft-deleted? — It is hidden from search results but existing applicants can still see it in their application history with a "Closed" label.
- What happens when a candidate updates their profile after applying? — The Match Score stored in the application is NOT recalculated. Profile changes only affect future applications.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to register as either a Candidate or a Recruiter, creating the appropriate user type in the system.
- **FR-002**: System MUST authenticate users and return a long-lived secure session token (e.g., 24-hour expiry) containing the user's role (Candidate / Recruiter / AdminRecruiter) for client-side routing. No refresh token mechanism is required.
- **FR-003**: System MUST support role-based access control — candidates, recruiters, and admin recruiters have distinct permission levels.
- **FR-004**: System MUST allow candidates to create and update their profile including job title, summary, experience years, and skills.
- **FR-005**: System MUST allow candidates to upload resumes in PDF or DOCX format and store them securely.
- **FR-006**: System MUST allow candidates to request AI-generated CV creation based on their profile data.
- **FR-007**: System MUST allow the first recruiter to create a company during registration (becoming Admin automatically), and allow admin recruiters to manage their company details, generate invite codes, and transfer admin role.
- **FR-008**: System MUST enforce unique tax numbers for company registration.
- **FR-009**: System MUST allow recruiters to create, edit, delete, publish, and unpublish job posts.
- **FR-010**: System MUST support job post drafts (unpublished state) and a publish action.
- **FR-011**: System MUST allow all users to search and browse published jobs with filters: location, job type, career level, industry, salary range — with pagination.
- **FR-012**: System MUST allow candidates to apply for published jobs only if they have an uploaded resume, recording the application with a timestamp and "Pending" status.
- **FR-013**: System MUST prevent duplicate applications — one application per candidate per job.
- **FR-014**: System MUST trigger AI resume scoring upon application submission, calculating a match score (0-100).
- **FR-015**: System MUST allow candidates to track the status of all their applications.
- **FR-016**: System MUST allow candidates to save/bookmark jobs (toggle on/off).
- **FR-017**: System MUST allow recruiters to view applicants for a job with match score, rating, and status — paginated and filterable.
- **FR-018**: System MUST allow recruiters to set a star rating (1-5) on job applications.
- **FR-019**: System MUST allow recruiters to export the applicant list as a CSV file.
- **FR-020**: System MUST allow recruiters to change application status through the pipeline in strict sequential order: Pending → UnderReview → Assessment → Interview → Accepted / Rejected. Stages cannot be skipped or reversed. A candidate may be Rejected at any stage. Candidates may Withdraw their own application while it is still Pending.
- **FR-021**: System MUST communicate with the AI service to analyze resumes, extract skills, process job descriptions, and calculate similarity scores.
- **FR-022**: System MUST allow recruiters to create assessments with questions of types: MCQ, True/False, Open-Ended, and Coding.
- **FR-023**: System MUST allow AI-generated assessment creation from job descriptions.
- **FR-024**: System MUST allow candidates to submit assessment answers within a defined time window and receive scores.
- **FR-025**: System MUST support scheduling of both AI and live interviews.
- **FR-026**: System MUST generate custom interview questions from job descriptions for text-based AI interviews. Candidates answer in writing and the AI scores responses.
- **FR-027**: System MUST support live video interviews via a third-party WebRTC provider (Daily.co / 100ms). Backend creates rooms and returns meeting links.
- **FR-028**: System MUST send email notifications for interview reminders, status changes, and acceptance/rejection.
- **FR-029**: System MUST deliver real-time in-app notifications for key events.
- **FR-030**: System MUST provide candidate dashboard data: applications count, saved jobs count, upcoming interviews, recent activity, and AI-suggested jobs (matched via the AI service against the candidate's full profile; gracefully omitted if AI service is unavailable).
- **FR-031**: System MUST provide company dashboard analytics: total applicants, active jobs, hire rate, application trends, and action-required items.
- **FR-032**: System MUST return applicants count in job detail responses.
- **FR-033**: System MUST return similar jobs for any given job using simple rule-based matching (shared skills, same career level, same industry) — without requiring the AI service.
- **FR-034**: System MUST gracefully handle AI service unavailability — accepting applications and queuing match scoring for retry.

### Key Entities

- **User**: A person who uses the platform. Can be a Candidate (job seeker) or a Recruiter (employer representative). All users share common identity information (name, email, phone, account status).
- **Candidate**: A job seeker who maintains a professional profile (job title, summary, experience), a set of skills, and one or more resumes. Candidates apply for jobs and track their applications.
- **Recruiter**: An employer representative who belongs to a company. Recruiters have organizational roles (Admin or Standard) that determine their permissions within the company.
- **Company**: An employer organization identified by a unique tax registration. Companies have recruiters, post jobs, and manage invite codes for onboarding new recruiters.
- **Job Post**: A job opportunity posted by a company — includes title, description, required skills, salary range, career level, and validity period. Can be in draft or published state.
- **Resume**: A candidate's career document — includes structured summaries of experience, education, and activities, plus the original uploaded file and any AI-enhanced version.
- **Job Application**: A candidate's application to a specific job — tracks the application date, current pipeline stage, AI match score, recruiter rating, and the resume submitted.
- **Skill**: A professional capability (e.g., a technology, language, or soft skill) that can be associated with candidates, job requirements, and resumes.
- **Assessment**: A structured evaluation for a job — can be technical, personality-based, or mixed. Contains questions and has a defined completion time window.
- **Question**: An individual assessment item — can be multiple choice, true/false, open-ended, or a coding challenge. Each has a defined correct answer and point value.
- **Interview**: A scheduled evaluation meeting for a candidate — can be conducted by AI (voice-based) or live (video call). Records scheduling, feedback, and scoring information.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete registration (candidate or recruiter) in under 2 minutes.
- **SC-002**: Authenticated users receive their session token and role within 2 seconds of submitting credentials.
- **SC-003**: Candidates can upload a resume and have it stored and linked to their profile within 5 seconds for files up to 10MB.
- **SC-004**: Job search with filters returns paginated results within 2 seconds for a database with 10,000+ job listings.
- **SC-005**: Upon job application submission, the AI scoring pipeline returns a match score within 30 seconds.
- **SC-006**: Recruiters can view a paginated list of applicants with scores and ratings within 2 seconds.
- **SC-007**: CSV export of applicant data for a job with 500+ applicants completes within 10 seconds.
- **SC-008**: Real-time interview connections are established within 5 seconds of both parties joining.
- **SC-009**: Email notifications (reminders, status changes, acceptance) are sent within 1 minute of the triggering event.
- **SC-010**: The system supports 200 concurrent users without performance degradation.
- **SC-011**: All protected endpoints correctly enforce role-based access — unauthorized requests are rejected 100% of the time.
- **SC-012**: The complete hiring pipeline (apply → score → assess → interview → accept) can be completed for a single candidate within one user session.
- **SC-013**: Dashboard data loads within 3 seconds and accurately reflects the current state of applications, jobs, and interviews.
- **SC-014**: 95% of candidate applications result in a successfully computed match score (accounting for AI service resilience).

## Assumptions

- The AI service is developed and deployed as a separate component; the backend communicates with it over a network protocol.
- User types (Candidate and Recruiter) share a common identity base but have distinct profile attributes and permissions.
- Authentication uses industry-standard token-based sessions with user roles embedded for authorization.
- A relational database is used for structured data storage with a schema-first approach.
- File uploads (resumes) are stored securely — the specific storage approach is an infrastructure decision.
- Email notifications are sent through a mail delivery service — the specific provider is an infrastructure decision.
- Real-time features (live interviews, in-app notifications) require a bidirectional communication channel and peer-to-peer media capabilities.
- The frontend is developed by a separate team and consumes the backend's REST APIs — the backend does not serve the user interface.
- The system follows a layered architecture with strict dependency rules to maintain separation of concerns.
- Only two recruiter roles exist: Admin and Standard. Junior Recruiter was evaluated and removed as unjustified complexity for MVP scope.
- AI-processed job description data is stored alongside the original description, not in a separate structure.
- Match scores are stored as decimal values for precision.
- Performance targets assume standard web application expectations unless otherwise specified.
- Error handling follows user-friendly message patterns with appropriate status indicators.
- Data retention follows standard practices — no specific regulatory requirements have been identified.

## Scope Boundaries

**In Scope**:
- User registration, authentication, and role-based access control
- Candidate profile and resume management
- Company registration and recruiter management
- Job posting, searching, and management
- Job applications with AI-powered scoring
- Applicant management, rating, and pipeline tracking
- Assessment creation, AI generation, and candidate evaluation
- Interview scheduling (AI voice + live video)
- Email and real-time notifications
- Candidate and company dashboards with analytics

**Out of Scope**:
- Frontend/UI development (handled by separate team)
- AI model training and ML pipeline development (AI service is a separate component)
- Payment processing or subscription management
- Internationalization/localization (single-language MVP)
- Platform-level super-admin panel — companies are created by recruiters during registration
- Candidate invitation by recruiters (future work)
- Messaging system between candidates and recruiters (future work)
- Mobile application backend-specific features
- Advanced dashboards — minimal aggregate numbers only, frontend aggregation preferred

## Dependencies

- **AI Service**: Resume scoring, skill extraction, assessment generation, and voice interview features depend on the AI service being available.
- **Database**: All data persistence depends on a relational database being provisioned and accessible.
- **Email Service**: Email notifications depend on a configured mail delivery service.
- **Real-time Communication Infrastructure**: Live interviews and real-time notifications depend on bidirectional communication and media streaming capabilities.
- **Frontend Team**: The frontend team must align on API contracts for integration.

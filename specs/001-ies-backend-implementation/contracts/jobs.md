# API Contract: Jobs

**Base Path**: `/api/jobs`  
**Authentication**: Mixed (public read, Recruiter write)

---

## POST /api/jobs

Create a new job post.

**Authorization**: Bearer JWT (Recruiter — Admin or Standard role)

**Request Body**:
```json
{
  "title": "string (required, max 200)",
  "description": "string (required, max 5000)",
  "location": "string? (max 200)",
  "jobType": "int (required, see JobType enum)",
  "workLocation": "int (required, see WorkLocation enum)",
  "careerLevel": "int (required, see JobLevel enum)",
  "salaryMin": "decimal? (≥ 0)",
  "salaryMax": "decimal? (≥ salaryMin)",
  "currency": "string? (max 10, default 'EGP')",
  "expiryDate": "string? (ISO 8601, must be future)",
  "isPublished": "bool (default false)",
  "skills": ["{ name: string, requiredLevel: int (1=Beginner, 2=Intermediate, 3=Expert) }"]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `JobDetailDto` | Created |
| 400 | Error | Validation error |

**Notes**:
- Job is automatically linked to the recruiter's company
- Skill names are matched against the Skills table (case-insensitive); unknown names return 400
- Both Admin and Standard recruiters can create jobs

---

## GET /api/jobs

Search and browse published jobs (public).

**Authorization**: None (public endpoint)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| search | string? | | Search title/description |
| location | string? | | Filter by location |
| jobType | int? | | Filter by JobType enum |
| workLocation | int? | | Filter by WorkLocation enum |
| careerLevel | int? | | Filter by JobLevel enum |
| industry | string? | | Filter by company industry |
| salaryMin | decimal? | | Minimum salary |
| salaryMax | decimal? | | Maximum salary |
| companyId | int? | | Filter by company |
| page | int | 1 | |
| pageSize | int | 10 | Max 50 |
| sortBy | string? | "createdAt" | createdAt, salaryMin, title |
| sortOrder | string? | "desc" | asc, desc |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<JobListDto>` | Success |

**JobListDto**:
```json
{
  "id": "int",
  "title": "string",
  "company": "{ id, name, logoPath }",
  "location": "string?",
  "jobType": "int",
  "workLocation": "int",
  "careerLevel": "int",
  "salaryMin": "decimal?",
  "salaryMax": "decimal?",
  "currency": "string?",
  "skills": ["{ id, name, requiredLevel }"],
  "applicantsCount": "int",
  "createdAt": "string",
  "expiryDate": "string?"
}
```

---

## GET /api/jobs/{jobId}

Get job post details.

**Authorization**: None (public — returns published jobs; Recruiter for own unpublished drafts)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `JobDetailDto` | Found |
| 404 | Error | Not found or not visible to user |

**JobDetailDto**:
```json
{
  "id": "int",
  "title": "string",
  "description": "string",
  "aiProcessedDescription": "string?",
  "location": "string?",
  "jobType": "int",
  "workLocation": "int",
  "careerLevel": "int",
  "salaryMin": "decimal?",
  "salaryMax": "decimal?",
  "currency": "string?",
  "expiryDate": "string?",
  "isPublished": "bool",
  "isActive": "bool",
  "company": "{ id, name, industry, logoPath }",
  "skills": ["{ id, name, requiredLevel, isRequired }"],
  "applicantsCount": "int",
  "similarJobs": ["{ id, title, companyName, location }"],
  "createdAt": "string",
  "updatedAt": "string?"
}
```

**Notes**:
- `similarJobs` uses rule-based matching (shared skills, same career level, same company industry)
- `applicantsCount` always included per FR-032

---

## PUT /api/jobs/{jobId}

Update a job post.

**Authorization**: Bearer JWT (Recruiter — must be creator or Admin of the same company)

**Request Body**: Same fields as POST (all optional for partial update)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `JobDetailDto` | Updated |
| 400 | Error | Validation error |
| 403 | Error | Not authorized for this job |
| 404 | Error | Job not found |

---

## DELETE /api/jobs/{jobId}

Soft-delete a job post.

**Authorization**: Bearer JWT (Recruiter — must be creator or Admin of the same company)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Soft-deleted |
| 403 | Error | Not authorized |
| 404 | Error | Not found |

**Notes**:
- Sets `DeletedAt` = now and `DeletedBy` = current user ID
- Soft-deleted jobs are excluded from all public queries and listing endpoints
- Associated applications are also soft-deleted
- This action is irreversible via API

---

## PATCH /api/jobs/{jobId}/publish

Publish or unpublish a job post.

**Authorization**: Bearer JWT (Recruiter — must be creator or Admin of the same company)

**Request Body**:
```json
{
  "isPublished": "bool (required)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ id, isPublished }` | Updated |
| 403 | Error | Not authorized |
| 404 | Error | Not found |

---

## GET /api/jobs/recruiter

Get jobs posted by the current recruiter's company.

**Authorization**: Bearer JWT (Recruiter)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| isPublished | bool? | | Filter by published state |
| page | int | 1 | |
| pageSize | int | 10 | Max 50 |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<JobListDto>` | Success |

---

## POST /api/jobs/{jobId}/apply

Apply to a job as a candidate.

**Authorization**: Bearer JWT (Candidate)

**Request Body**:
```json
{
  "resumeId": "int (required, must be candidate's own resume)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ applicationId, status, appliedAt }` | Application created, AI scoring triggered |
| 400 | Error | No resume uploaded / resume not found |
| 404 | Error | Job not found or not published |
| 409 | Error | Already applied to this job |

**Notes**: AI match scoring is triggered asynchronously. If the AI service is unavailable, the application is still created with `matchScore = null`.

---

## GET /api/jobs/{jobId}/applicants

Get applicants for a job.

**Authorization**: Bearer JWT (Recruiter — must belong to the job's company)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| status | int? | | Filter by ApplicationStatus |
| minScore | decimal? | | Minimum match score |
| sortBy | string? | "matchScore" | matchScore, appliedAt, rating |
| sortOrder | string? | "desc" | asc, desc |
| page | int | 1 | |
| pageSize | int | 10 | Max 50 |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<ApplicantDto>` | Success |
| 403 | Error | Not authorized for this job |

**ApplicantDto**:
```json
{
  "applicationId": "int",
  "candidate": "{ id, firstName, lastName, email, jobTitle, yearsOfExperience }",
  "status": "int",
  "matchScore": "decimal?",
  "matchReport": "string?",
  "recruiterRating": "int?",
  "appliedAt": "string",
  "updatedAt": "string?"
}
```

---

## PATCH /api/jobs/{jobId}/applicants/{applicationId}/status

Change application status (move through pipeline).

**Authorization**: Bearer JWT (Recruiter — must belong to the job's company)

**Request Body**:
```json
{
  "newStatus": "int (required, see ApplicationStatus enum)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ applicationId, previousStatus, newStatus }` | Transitioned |
| 400 | Error | Invalid transition (skipping stage, backwards move, or terminal status) |
| 403 | Error | Not authorized |
| 404 | Error | Application not found |

**Notes**: Enforces strict sequential pipeline. See data-model.md for valid transitions. Withdrawn (6) is a terminal state that can only be set by the candidate via the withdraw endpoint (see below).

---

## POST /api/jobs/{jobId}/applicants/{applicationId}/withdraw

Withdraw a job application (candidate only).

**Authorization**: Bearer JWT (Candidate — must own this application)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ applicationId, previousStatus, newStatus: 6 }` | Withdrawn |
| 400 | Error | Application is not in Pending status |
| 403 | Error | Not the applicant |
| 404 | Error | Application not found |

**Notes**:
- Only applications in `Pending` (0) status can be withdrawn
- Withdrawn is a terminal state — no further transitions allowed
- The candidate cannot re-apply to the same job after withdrawing

---

## PATCH /api/jobs/{jobId}/applicants/{applicationId}/rating

Set recruiter rating on an application.

**Authorization**: Bearer JWT (Recruiter — must belong to the job's company)

**Request Body**:
```json
{
  "rating": "int (required, 1-5)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ applicationId, rating }` | Updated |
| 400 | Error | Rating out of range |
| 403 | Error | Not authorized |

---

## GET /api/jobs/{jobId}/applicants/export

Export applicants list as CSV.

**Authorization**: Bearer JWT (Recruiter — must belong to the job's company)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | CSV file (`text/csv`, Content-Disposition: attachment) | Success |
| 403 | Error | Not authorized |

**CSV Columns**: Candidate Name, Email, Job Title, Applied Date, Status, Rating, Match Score

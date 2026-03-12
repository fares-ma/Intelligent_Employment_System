# API Contract: Candidates

**Base Path**: `/api/candidates`  
**Authentication**: Bearer JWT (Candidate role unless noted)

---

## GET /api/candidates/profile

Get the current candidate's profile.

**Authorization**: Bearer JWT (Candidate)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `CandidateProfileDto` | Success |
| 401 | Error | Not authenticated |
| 403 | Error | Not a candidate |

**CandidateProfileDto**:
```json
{
  "id": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phoneNumber": "string?",
  "gender": "int",
  "dateOfBirth": "string?",
  "profilePicturePath": "string?",
  "jobTitle": "string?",
  "summary": "string?",
  "yearsOfExperience": "int?",
  "careerLevel": "int?",
  "linkedInUrl": "string?",
  "portfolioUrl": "string?",
  "address": "string?",
  "city": "string?",
  "country": "string?",
  "skills": ["{ id, name, category, level }"],
  "education": ["{ id, degree, fieldOfStudy, institution, graduationYear }"],
  "experience": ["{ id, jobTitle, company, description, startDate, endDate }"],
  "resumes": ["{ id, originalFileName, fileType, isDefault, createdAt }"]
}
```

**Notes**:
- `skills[].level` is the SkillLevel enum: 1=Beginner, 2=Intermediate, 3=Expert
- `education` and `experience` are managed via separate CRUD endpoints (see below)

---

## PUT /api/candidates/profile

Update the current candidate's profile.

**Authorization**: Bearer JWT (Candidate)

**Request Body**:
```json
{
  "firstName": "string? (max 100)",
  "lastName": "string? (max 100)",
  "phoneNumber": "string? (max 20)",
  "jobTitle": "string? (max 200)",
  "summary": "string? (max 2000)",
  "yearsOfExperience": "int? (≥ 0)",
  "careerLevel": "int?",
  "linkedInUrl": "string? (max 500)",
  "portfolioUrl": "string? (max 500)",
  "address": "string? (max 500)",
  "city": "string? (max 100)",
  "country": "string? (max 100)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `CandidateProfileDto` | Updated |
| 400 | Error | Validation error |

---

## PUT /api/candidates/skills

Set the candidate's skills with levels (replaces entire existing set).

**Authorization**: Bearer JWT (Candidate)

**Request Body**:
```json
{
  "skills": [
    { "name": "string (required)", "level": "int (required, 1=Beginner, 2=Intermediate, 3=Expert)" }
  ]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `[{ id, name, category, level }]` | Skills updated |
| 400 | Error | Invalid skill names or level values |

**Notes**:
- Replace-all semantics: existing skills are removed and replaced with the provided list
- Skill names are matched against the Skills table (case-insensitive); unknown names return 400
- Level must be a valid SkillLevel enum value (1, 2, or 3)

---

## POST /api/candidates/education

Add an education entry.

**Authorization**: Bearer JWT (Candidate)

**Request Body**:
```json
{
  "degree": "string (required, max 200)",
  "fieldOfStudy": "string (required, max 200)",
  "institution": "string (required, max 300)",
  "graduationYear": "int? (e.g. 2023)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ id, degree, fieldOfStudy, institution, graduationYear }` | Created |
| 400 | Error | Validation error |

---

## PUT /api/candidates/education/{educationId}

Update an education entry.

**Authorization**: Bearer JWT (Candidate, must own the entry)

**Request Body**: Same fields as POST (all optional for partial update)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ id, degree, fieldOfStudy, institution, graduationYear }` | Updated |
| 400 | Error | Validation error |
| 404 | Error | Not found or not owned |

---

## DELETE /api/candidates/education/{educationId}

Delete an education entry.

**Authorization**: Bearer JWT (Candidate, must own the entry)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Deleted |
| 404 | Error | Not found or not owned |

---

## POST /api/candidates/experience

Add a work experience entry.

**Authorization**: Bearer JWT (Candidate)

**Request Body**:
```json
{
  "jobTitle": "string (required, max 200)",
  "company": "string (required, max 200)",
  "description": "string? (max 500)",
  "startDate": "string (required, ISO 8601 date)",
  "endDate": "string? (ISO 8601 date, must be after startDate)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ id, jobTitle, company, description, startDate, endDate }` | Created |
| 400 | Error | Validation error |

**Notes**:
- `endDate` is null for current positions

---

## PUT /api/candidates/experience/{experienceId}

Update a work experience entry.

**Authorization**: Bearer JWT (Candidate, must own the entry)

**Request Body**: Same fields as POST (all optional for partial update)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ id, jobTitle, company, description, startDate, endDate }` | Updated |
| 400 | Error | Validation error |
| 404 | Error | Not found or not owned |

---

## DELETE /api/candidates/experience/{experienceId}

Delete a work experience entry.

**Authorization**: Bearer JWT (Candidate, must own the entry)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Deleted |
| 404 | Error | Not found or not owned |

---

## POST /api/candidates/resume

Upload a resume file.

**Authorization**: Bearer JWT (Candidate)

**Request**: `multipart/form-data`
| Field | Type | Constraints |
|-------|------|-------------|
| file | IFormFile | Required, PDF/DOCX only, ≤10MB |
| isDefault | bool | Optional, default false |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ id, originalFileName, fileType, fileSizeBytes, isDefault, createdAt }` | Uploaded |
| 400 | Error | Invalid file type, too large |
| 413 | Error | File exceeds size limit |

---

## DELETE /api/candidates/resume/{resumeId}

Delete a resume.

**Authorization**: Bearer JWT (Candidate, must own the resume)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Deleted |
| 404 | Error | Resume not found or not owned |

---

## POST /api/candidates/resume/{resumeId}/generate-cv

Request AI-generated CV from a resume.

**Authorization**: Bearer JWT (Candidate, must own the resume)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ aiGeneratedCvPath }` | CV generated |
| 404 | Error | Resume not found |
| 503 | Error | AI service unavailable |

---

## PUT /api/candidates/profile-picture

Upload or update profile picture.

**Authorization**: Bearer JWT (Candidate)

**Request**: `multipart/form-data`
| Field | Type | Constraints |
|-------|------|-------------|
| file | IFormFile | Required, image (JPG/PNG), ≤5MB |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ profilePicturePath }` | Updated |
| 400 | Error | Invalid file type or too large |

---

## GET /api/candidates/applications

Get all applications for the current candidate.

**Authorization**: Bearer JWT (Candidate)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| page | int | 1 | |
| pageSize | int | 10 | Max 50 |
| status | int? | | Filter by ApplicationStatus |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<CandidateApplicationDto>` | Success |

**CandidateApplicationDto**:
```json
{
  "id": "int",
  "jobPost": "{ id, title, companyName, location }",
  "status": "int",
  "matchScore": "decimal?",
  "appliedAt": "string (ISO 8601)",
  "updatedAt": "string?"
}
```

---

## GET /api/candidates/saved-jobs

Get candidate's saved/bookmarked jobs.

**Authorization**: Bearer JWT (Candidate)

**Query Parameters**: `page`, `pageSize`

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<SavedJobDto>` | Success |

---

## POST /api/candidates/saved-jobs/{jobPostId}

Toggle save/unsave a job.

**Authorization**: Bearer JWT (Candidate)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ isSaved: true/false }` | Toggled |
| 404 | Error | Job not found |

---

## GET /api/candidates/dashboard

Get candidate dashboard data.

**Authorization**: Bearer JWT (Candidate)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `CandidateDashboardDto` | Success |

**CandidateDashboardDto**:
```json
{
  "applicationsCount": "int",
  "savedJobsCount": "int",
  "upcomingInterviews": ["{ id, jobTitle, scheduledAt, interviewType }"],
  "recentActivity": ["{ type, description, timestamp }"],
  "aiSuggestedJobs": ["{ id, title, companyName, matchScore }"]
}
```

**Notes**: `aiSuggestedJobs` is omitted (empty array) if AI service is unavailable.

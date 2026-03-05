# API Contract: Assessments

**Base Path**: `/api/assessments`  
**Authentication**: Bearer JWT

---

## POST /api/assessments

Create a new assessment for a job.

**Authorization**: Bearer JWT (Recruiter — must own the job or be Admin of the company)

**Request Body**:
```json
{
  "jobPostId": "int (required)",
  "title": "string (required, max 200)",
  "description": "string? (max 2000)",
  "type": "int (required, see AssessmentType enum)",
  "timeLimitMinutes": "int (required, > 0)",
  "isAiGenerated": "bool (default false)",
  "questions": [
    {
      "text": "string (required, max 2000)",
      "type": "int (required, see QuestionType enum)",
      "options": "string? (JSON array for MCQ)",
      "correctAnswer": "string? (max 2000)",
      "points": "int (required, > 0)",
      "orderIndex": "int (required)"
    }
  ]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `AssessmentDetailDto` | Created |
| 400 | Error | Validation error (no questions, invalid types) |
| 403 | Error | Not authorized for this job |
| 404 | Error | Job not found |

**Notes**: `totalScore` is auto-calculated from question points.

---

## GET /api/assessments/{assessmentId}

Get assessment details.

**Authorization**: Bearer JWT (Recruiter who owns the job, OR Candidate at the Assessment stage for this job)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `AssessmentDetailDto` | Found |
| 403 | Error | Not authorized |
| 404 | Error | Not found |

**AssessmentDetailDto**:
```json
{
  "id": "int",
  "jobPostId": "int",
  "title": "string",
  "description": "string?",
  "type": "int",
  "timeLimitMinutes": "int",
  "totalScore": "int",
  "isAiGenerated": "bool",
  "isActive": "bool",
  "questions": [
    {
      "id": "int",
      "text": "string",
      "type": "int",
      "options": "string?",
      "correctAnswer": "string? (hidden from candidates)",
      "points": "int",
      "orderIndex": "int"
    }
  ],
  "createdAt": "string"
}
```

**Notes**: `correctAnswer` is hidden from candidate responses — only visible to recruiters.

---

## PUT /api/assessments/{assessmentId}

Update an assessment.

**Authorization**: Bearer JWT (Recruiter — must own the job or be Admin)

**Request Body**: Same fields as POST (all optional for partial update)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `AssessmentDetailDto` | Updated |
| 400 | Error | Validation error |
| 403 | Error | Not authorized |

---

## DELETE /api/assessments/{assessmentId}

Delete an assessment.

**Authorization**: Bearer JWT (Recruiter — must own the job or be Admin)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Deleted |
| 403 | Error | Not authorized |
| 404 | Error | Not found |

---

## GET /api/assessments/job/{jobPostId}

List assessments for a job.

**Authorization**: Bearer JWT (Recruiter belonging to the job's company)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `[{ id, title, type, timeLimitMinutes, totalScore, isAiGenerated, isActive }]` | Success |
| 403 | Error | Not authorized |

---

## POST /api/assessments/{assessmentId}/start

Start an assessment attempt (candidate).

**Authorization**: Bearer JWT (Candidate — must be at Assessment stage for the related job)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ candidateAssessmentId, startedAt, deadlineAt, questions }` | Started |
| 400 | Error | Already attempted this assessment |
| 403 | Error | Not at assessment stage |
| 404 | Error | Assessment not found |

**Notes**:
- `deadlineAt` = `startedAt` + `timeLimitMinutes`
- Returns questions without `correctAnswer`

---

## POST /api/assessments/{assessmentId}/submit

Submit assessment answers (candidate).

**Authorization**: Bearer JWT (Candidate — must have a started attempt)

**Request Body**:
```json
{
  "answers": [
    {
      "questionId": "int (required)",
      "answer": "string (required)"
    }
  ]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ candidateAssessmentId, score, totalScore, isCompleted }` | Submitted & scored |
| 400 | Error | Time window expired |
| 403 | Error | Not authorized or no active attempt |

**Notes**: 
- Submission is rejected if current time > `deadlineAt`
- MCQ and True/False are auto-graded; Open-Ended and Coding may need manual review

---

## GET /api/assessments/{assessmentId}/results

Get assessment results for all candidates (recruiter).

**Authorization**: Bearer JWT (Recruiter belonging to the job's company)

**Query Parameters**: `page`, `pageSize`

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<CandidateAssessmentResultDto>` | Success |

**CandidateAssessmentResultDto**:
```json
{
  "candidateAssessmentId": "int",
  "candidate": "{ id, firstName, lastName, email }",
  "score": "decimal?",
  "totalScore": "int",
  "startedAt": "string?",
  "submittedAt": "string?",
  "isCompleted": "bool"
}
```

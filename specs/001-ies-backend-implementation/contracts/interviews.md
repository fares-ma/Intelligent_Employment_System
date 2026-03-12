# API Contract: Interviews

**Base Path**: `/api/interviews`  
**Authentication**: Bearer JWT

---

## POST /api/interviews

Schedule an interview.

**Authorization**: Bearer JWT (Recruiter — must belong to the job's company)

**Request Body**:
```json
{
  "jobApplicationId": "int (required)",
  "interviewType": "int (required, 0=AI, 1=Live)",
  "scheduledAt": "string (required, ISO 8601, must be future)",
  "durationMinutes": "int? (default 30, max 120)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `InterviewDetailDto` | Created |
| 400 | Error | Candidate not at Interview stage, or past date |
| 403 | Error | Not authorized for this application |
| 404 | Error | Application not found |

**Notes**:
- Application must be at `Interview` status
- For live interviews: a `meetingLink` is auto-generated
- Both candidate and recruiter receive notifications

---

## GET /api/interviews/{interviewId}

Get interview details.

**Authorization**: Bearer JWT (Recruiter of the company OR the candidate for this application)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `InterviewDetailDto` | Found |
| 403 | Error | Not authorized |
| 404 | Error | Not found |

**InterviewDetailDto**:
```json
{
  "id": "int",
  "jobApplication": "{ id, jobTitle, candidateName }",
  "interviewType": "int",
  "status": "int",
  "scheduledAt": "string",
  "durationMinutes": "int",
  "meetingLink": "string? (live interviews only)",
  "score": "decimal?",
  "feedbackNotes": "string?",
  "aiAnswers": "string? (AI interviews only, JSON of Q&A pairs, recruiter-visible)",
  "completedAt": "string?",
  "createdAt": "string"
}
```

---

## GET /api/interviews

List interviews for current user.

**Authorization**: Bearer JWT (Recruiter or Candidate)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| status | int? | | Filter by InterviewStatus |
| interviewType | int? | | Filter by type |
| upcoming | bool? | true | Only future interviews |
| page | int | 1 | |
| pageSize | int | 10 | Max 50 |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<InterviewListDto>` | Success |

**Notes**: 
- Candidates see their own interviews
- Recruiters see interviews for their company's jobs

---

## POST /api/interviews/{interviewId}/ai-questions

Get AI-generated interview questions (text-based).

**Authorization**: Bearer JWT (Candidate — must be the interviewee)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, questions: [{ id: int, text: string }], status: "InProgress" }` | Questions generated |
| 400 | Error | Not within allowed time window (15min before to scheduled+duration) |
| 400 | Error | Interview is not AI type or not in Scheduled status |
| 503 | Error | AI service unavailable |

**Notes**:
- Questions are generated based on the job description and candidate profile
- Cannot start more than 15 minutes before scheduled time
- Sets interview status to InProgress
- If AI service is unavailable, returns predefined fallback questions

---

## POST /api/interviews/{interviewId}/submit-ai

Submit written answers for an AI interview.

**Authorization**: Bearer JWT (Candidate — must be the interviewee)

**Request Body**:
```json
{
  "answers": [
    { "questionId": "int", "answer": "string (required, max 2000)" }
  ]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, score, feedback, status: "Completed" }` | Scored |
| 400 | Error | Interview not InProgress or missing answers |
| 503 | Error | AI service unavailable for scoring |

**Notes**:
- All questions must be answered
- AI evaluates written answers and returns a score (0-100) and textual feedback
- Answers are stored as JSON in the `AiAnswers` field
- If AI scoring is unavailable, answers are saved but score remains null

---

## POST /api/interviews/{interviewId}/join

Get the meeting link for a live interview.

**Authorization**: Bearer JWT (Candidate or Recruiter — must be participant)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, meetingLink }` | Meeting link returned |
| 400 | Error | Not within allowed time window |
| 400 | Error | Interview is not Live type |
| 403 | Error | Not a participant |

**Notes**: 
- Meeting link is auto-generated when the interview is created (via third-party service like Daily.co / 100ms)
- Cannot join more than 15 minutes before scheduled time
- Backend creates the room via third-party API and stores the meeting link

---

## PATCH /api/interviews/{interviewId}/complete

Complete a live interview with feedback.

**Authorization**: Bearer JWT (Recruiter — must be the interviewer's company)

**Request Body**:
```json
{
  "score": "decimal? (0-100)",
  "feedbackNotes": "string? (max 2000)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `InterviewDetailDto` | Completed |
| 400 | Error | Interview not InProgress |
| 403 | Error | Not authorized |

---

## PATCH /api/interviews/{interviewId}/cancel

Cancel a scheduled interview.

**Authorization**: Bearer JWT (Recruiter — must belong to the job's company)

**Request Body**:
```json
{
  "reason": "string? (max 500)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, status: "Cancelled" }` | Cancelled |
| 400 | Error | Interview not in Scheduled status |
| 403 | Error | Not authorized |


# API Contract: AI Service Integration

**Base Path**: `/api/ai`  
**Authentication**: Bearer JWT (varies by endpoint)

These endpoints act as proxies to the external Python FastAPI AI microservice. The backend orchestrates the calls, handles resilience (retry + circuit breaker), and stores results.

---

## POST /api/ai/extract-skills

Extract structured skills from a free-text job description.

**Authorization**: Bearer JWT (Recruiter)

**Request Body**:
```json
{
  "text": "string (required, max 5000, free-text job description)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ skills: [{ name, category, isRequired }], summary }` | Extracted |
| 400 | Error | Empty or invalid text |
| 503 | Error | AI service unavailable |

---

## POST /api/ai/analyze-resume

Analyze a resume against job requirements (triggered internally on application, but can be called directly for re-scoring).

**Authorization**: Bearer JWT (Recruiter — must own the job)

**Request Body**:
```json
{
  "resumeId": "int (required)",
  "jobPostId": "int (required)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ matchScore, matchReport, skillsGap: [{ skill, status }] }` | Analyzed |
| 404 | Error | Resume or job not found |
| 503 | Error | AI service unavailable |

---

## POST /api/ai/generate-assessment

Generate assessment questions from a job description.

**Authorization**: Bearer JWT (Recruiter)

**Request Body**:
```json
{
  "jobPostId": "int (required)",
  "questionCount": "int? (default 10, max 30)",
  "questionTypes": ["int? (array of QuestionType enum values, default all types)"]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ questions: [{ text, type, options?, correctAnswer?, points }] }` | Generated |
| 404 | Error | Job not found |
| 503 | Error | AI service unavailable |

---

## POST /api/ai/generate-cv

Generate an AI-enhanced CV for a candidate.

**Authorization**: Bearer JWT (Candidate — own resume only)

**Request Body**:
```json
{
  "resumeId": "int (required)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ cvPath, summary }` | Generated |
| 404 | Error | Resume not found |
| 503 | Error | AI service unavailable |

---

## POST /api/ai/interview-questions

Generate interview questions from a job description.

**Authorization**: Bearer JWT (Recruiter — or system-triggered for AI interviews)

**Request Body**:
```json
{
  "jobPostId": "int (required)",
  "questionCount": "int? (default 5, max 15)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ questions: [{ text, expectedTopics: [string] }] }` | Generated |
| 404 | Error | Job not found |
| 503 | Error | AI service unavailable |

---

## POST /api/ai/score-interview

Score an AI voice interview transcript.

**Authorization**: Bearer JWT (system-internal, triggered after AI interview)

**Request Body**:
```json
{
  "interviewId": "int (required)",
  "transcript": "string (required)",
  "questions": ["{ text, expectedTopics }"]
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ score, feedback, detailedScores: [{ question, score, notes }] }` | Scored |
| 503 | Error | AI service unavailable |

---

## Resilience Behavior

All AI endpoints follow this resilience pipeline:
1. **Retry**: 3 attempts, exponential backoff (2s, 4s, 8s)
2. **Circuit Breaker**: Opens after 50% failure rate in 30s window, stays open 30s
3. **Timeout**: 45s per request, 60s on HttpClient

When the AI service is unavailable:
- Application-triggered scoring: application is created with `matchScore = null`, queued for retry
- Direct AI calls: return 503 with message "AI service is temporarily unavailable. Please try again later."

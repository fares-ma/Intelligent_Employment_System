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
  "aiTranscript": "string? (AI interviews only, recruiter-visible)",
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

## POST /api/interviews/{interviewId}/start-ai

Start an AI voice interview.

**Authorization**: Bearer JWT (Candidate — must be the interviewee)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, questions: [{ text, expectedTopics }], status: "InProgress" }` | Started |
| 400 | Error | Not within allowed time window (15min before to scheduled+duration) |
| 400 | Error | Interview is not AI type or not in Scheduled status |
| 503 | Error | AI service unavailable |

**Notes**: Cannot start more than 15 minutes before scheduled time.

---

## POST /api/interviews/{interviewId}/complete-ai

Complete an AI voice interview with transcript.

**Authorization**: Bearer JWT (Candidate — must be the interviewee)

**Request Body**:
```json
{
  "transcript": "string (required, max 10000)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, score, feedback, status: "Completed" }` | Scored |
| 400 | Error | Interview not InProgress |
| 503 | Error | AI service unavailable for scoring |

---

## POST /api/interviews/{interviewId}/join

Join a live interview room (both candidate and recruiter).

**Authorization**: Bearer JWT (Candidate or Recruiter — must be participant)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ interviewId, meetingLink, signalingHubUrl, roomId }` | Joined |
| 400 | Error | Not within allowed time window |
| 400 | Error | Interview is not Live type |
| 403 | Error | Not a participant |

**Notes**: 
- Returns SignalR hub URL for WebRTC signaling
- Cannot join more than 15 minutes before scheduled time

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

---

## SignalR Hub: /hubs/interview

WebRTC signaling for live interviews.

**Authentication**: JWT via `access_token` query parameter

**Client → Server Methods**:
| Method | Parameters | Description |
|--------|-----------|-------------|
| JoinRoom | `interviewId: int` | Join interview signaling room |
| LeaveRoom | `interviewId: int` | Leave interview signaling room |
| SendOffer | `interviewId: int, sdp: string` | Send WebRTC SDP offer |
| SendAnswer | `interviewId: int, sdp: string` | Send WebRTC SDP answer |
| SendIceCandidate | `interviewId: int, candidate: string` | Forward ICE candidate |

**Server → Client Methods**:
| Method | Parameters | Description |
|--------|-----------|-------------|
| UserJoined | `userId: string, name: string` | Other participant joined |
| UserLeft | `userId: string` | Other participant left |
| ReceiveOffer | `sdp: string` | Receive WebRTC offer |
| ReceiveAnswer | `sdp: string` | Receive WebRTC answer |
| ReceiveIceCandidate | `candidate: string` | Receive ICE candidate |

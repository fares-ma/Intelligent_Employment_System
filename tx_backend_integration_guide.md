# TalentX AI Resume Scorer Integration Guide

This guide provides technical documentation for integrating the **TalentX AI Resume Scorer Service** with the backend system.

---

## Overview

The TalentX AI matching engine processes CV files asynchronously against specific Job Descriptions (JDs), saves results locally, and notifies the backend when processing completes or fails.

The backend team can interact with the AI service in two ways:
1. **Asynchronous Request/Response Flow (Recommended)**: Initiate scoring using the `POST /api/v1/score` endpoint, and receive the results via a webhook payload.
2. **Polling Flow**: Initiate scoring via `POST /api/v1/score` and periodically query the results via `GET /api/v1/results/{candidate_id}/{job_id}`.

---

## API Endpoints

### 1. Score Resume (Trigger Matching)
Initiate the parsing, normalization, and similarity scoring pipeline in the background.

* **Endpoint**: `POST /api/v1/score`
* **Content-Type**: `multipart/form-data`
* **Response Status**: `202 Accepted`

#### Request Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `candidate_id` | Integer | Yes | Unique ID of the candidate. |
| `job_id` | Integer | Yes | Unique ID of the job post. |
| `cv_file` | File | Yes | The CV document file. Supported extensions: `.pdf`, `.docx`, `.doc`, `.txt`. |
| `job_description` | String (JSON) | Yes | Stringified JSON representing the job requirements. |

> [!IMPORTANT]
> The `job_description` parameter must be a valid JSON string matching the [Job Description JSON Schema](#job-description-json-schema).

#### Request Example (cURL)
```bash
curl -X POST "http://localhost:8008/api/v1/score" \
  -H "Content-Type: multipart/form-data" \
  -F "candidate_id=999" \
  -F "job_id=888" \
  -F "cv_file=@/path/to/resume.pdf" \
  -F "job_description={...}"
```

#### Response Example
```json
{
  "success": true,
  "message": "Resume matching started in background",
  "candidate_id": 999,
  "job_id": 888,
  "status": "processing"
}
```

---

### 2. Get Score Results (Polling)
Check status or retrieve the completed scores for a specific candidate and job evaluation.

* **Endpoint**: `GET /api/v1/results/{candidate_id}/{job_id}`
* **Response Status**: `200 OK` (Success) or `404 Not Found` (No record found)

#### Response Example (Success)
```json
{
  "success": true,
  "candidate_id": 999,
  "job_id": 888,
  "status": "completed",
  "final_score": 95.6,
  "fit_status": "fit",
  "raw_resume_data": {
    "name": "Ahmed G3bl",
    "email": "G3blG3bolElGamed@gmail.com",
    "phone": "0101111002",
    "total_exp": 1.25,
    "skills": ["C#", "Entity Framework", "SQL", "Git"],
    "degree": ["Bachelor of Computer Science"],
    "university": ["ain shams university"],
    "experience": [
      {
        "title": "Backend .NET Developer Intern",
        "company": "Stock Squares",
        "date_range": "Feb 2026 – Present",
        "description": "Contributed to the development of the StockSquares backend system..."
      }
    ],
    "education": [
      {
        "degree": "Bachelor of Computer Science",
        "university": "",
        "gpa": "3.47",
        "date_range": "Oct 2022 - Jul 2026"
      }
    ]
  },
  "matching_details": {
    "final_score": 95.6,
    "fit_status": "fit",
    "skills": {
      "score": 1.0,
      "priority": "High",
      "details": [
        {
          "jd_skill": "C#",
          "best_cv_match": "C#",
          "similarity": 1.0,
          "priority": "High",
          "category": "must_have"
        }
      ]
    },
    "education": {
      "score": 1.0,
      "priority": "High",
      "details": [
        {
          "field": "degree",
          "jd_requirement": "Computer Science",
          "best_cv_match": "Computer Science",
          "similarity": 1.0,
          "priority": "High"
        }
      ]
    }
  },
  "error_message": null,
  "created_at": "2026-05-21 16:06:42",
  "updated_at": "2026-05-21 16:06:42"
}
```

---

### 3. Service Health Check
Verify the availability of the AI service.

* **Endpoint**: `GET /health`
* **Response Status**: `200 OK`

#### Response Example
```json
{
  "status": "healthy"
}
```

---

## Webhook Callback

* **Method**: `POST`
* **Headers**:
  * `Content-Type`: `application/json`
  * `X-Idempotency-Key`: `talentx-match-{candidate_id}-{job_id}-{timestamp}` (Use this key to guarantee idempotency, check details below).

### Idempotency & Retries
> [!TIP]
> **Deduplication Recommendation**: The backend must track the received `X-Idempotency-Key` or `idempotency_key` field in the database.
> If the backend receives multiple requests with the same key (e.g. due to retry on network timeouts), it should return a `200 OK` without processing the webhook body a second time.
> A change in the timestamp component of the key indicates a brand new execution run triggered by the user (meaning it should be processed).

### Webhook Payloads

#### Case A: Processing Completed Successfully
```json
{
  "candidate_id": 999,
  "job_id": 888,
  "status": "completed",
  "final_score": 95.6,
  "fit_status": "fit",
  "raw_resume_data": {
    "name": "Ahmed G3bl",
    "email": "G3blG3bolElGamed@gmail.com",
    "phone": "0101111002",
    "total_exp": 1.25,
    "skills": ["C#", "Entity Framework", "SQL"],
    "degree": ["Bachelor of Computer Science"],
    "university": ["ain shams university"]
  },
  "matching_details": {
    "final_score": 95.6,
    "fit_status": "fit",
    "skills": {
      "score": 1.0,
      "priority": "High",
      "details": [
        {
          "jd_skill": "C#",
          "best_cv_match": "C#",
          "similarity": 1.0,
          "priority": "High",
          "category": "must_have"
        }
      ]
    }
  },
  "error_message": null,
  "idempotency_key": "talentx-match-999-888-2026-05-21T16:06:42"
}
```

#### Case B: Processing Failed
```json
{
  "candidate_id": 999,
  "job_id": 888,
  "status": "failed",
  "final_score": null,
  "fit_status": null,
  "raw_resume_data": null,
  "matching_details": null,
  "error_message": "Unsupported file format or corrupted file layout.",
  "idempotency_key": "talentx-match-999-888-2026-05-21T16:07:10"
}
```

---

## Job Description JSON Schema

The `job_description` field passed to `POST /api/v1/score` must adhere to the structure below:

```json
{
  "job_title": "Backend Software Engineering Intern (ASP.NET Core)",
  "education": {
    "description": "Currently pursuing or recently graduated with a degree in Computer Science, Software Engineering...",
    "priority": "High",
    "required_degrees": [
      {"name": "Computer Science", "priority": "High"},
      {"name": "Software Engineering", "priority": "High"}
    ],
    "gpa": {
      "min": 2.5,
      "priority": "Low"
    }
  },
  "experience": {
    "description": "Internship-level position. No prior professional experience required.",
    "priority": "Low",
    "roles": [
      {"name": "Backend Developer", "priority": "High"},
      {"name": "Software Engineer", "priority": "High"}
    ],
    "years": {
      "min": 0,
      "max": 2,
      "priority": "Low"
    }
  },
  "skills": {
    "description": "Required technical skills for ASP.NET Core backend development.",
    "priority": "High",
    "must_have": [
      {"name": "C#", "priority": "High"},
      {"name": "ASP.NET Core", "priority": "High"},
      {"name": "Entity Framework Core", "priority": "High"},
      {"name": "SQL", "priority": "High"},
      {"name": "Git", "priority": "High"}
    ],
    "nice_to_have": [
      {"name": "RESTful APIs", "priority": "Low"},
      {"name": "Unit Testing", "priority": "Low"}
    ]
  }
}
```

### Schema Rules & Priorities:
- **Priority Fields**: Accept values `"High"` or `"Low"`. These weights are used during similarity S-BERT calculation to prioritize different parts of the CV evaluation.
- **Degrees and Roles**: Specify preferred keyword targets matching candidates' backgrounds.
- **Skills Categories**:
  - `must_have`: Critical skills; lacking matches significantly impacts candidate compatibility.
  - `nice_to_have`: Optional skills; counted as bonus features in evaluation metrics.

# API Contract: Companies

**Base Path**: `/api/companies`  
**Authentication**: Mixed (public read, authenticated write)

---

## POST /api/companies

Create a new company (super-admin only).

**Authorization**: Bearer JWT (Admin role — platform super-admin)

**Request Body**:
```json
{
  "name": "string (required, max 200)",
  "industry": "string? (max 100)",
  "website": "string? (max 500)",
  "taxNumber": "string (required, max 50)",
  "phoneNumber": "string? (max 20)",
  "description": "string? (max 2000)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `CompanyDto` | Created |
| 400 | Error | Validation error |
| 409 | Error | TaxNumber already exists |

---

## GET /api/companies/{companyId}

Get company details (public).

**Authorization**: None (public endpoint)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `CompanyDetailDto` | Found |
| 404 | Error | Company not found |

**CompanyDetailDto**:
```json
{
  "id": "int",
  "name": "string",
  "industry": "string?",
  "website": "string?",
  "phoneNumber": "string?",
  "description": "string?",
  "logoPath": "string?",
  "isVerified": "bool",
  "jobCount": "int",
  "createdAt": "string"
}
```

---

## GET /api/companies

List companies with search (public).

**Authorization**: None (public endpoint)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| search | string? | | Name/industry search |
| page | int | 1 | |
| pageSize | int | 10 | Max 50 |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<CompanyDto>` | Success |

---

## PUT /api/companies/{companyId}

Update company details.

**Authorization**: Bearer JWT (AdminRecruiter belonging to this company)

**Request Body**:
```json
{
  "name": "string? (max 200)",
  "industry": "string? (max 100)",
  "website": "string? (max 500)",
  "phoneNumber": "string? (max 20)",
  "description": "string? (max 2000)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `CompanyDto` | Updated |
| 400 | Error | Validation error |
| 403 | Error | Not admin of this company |
| 404 | Error | Company not found |

**Notes**: `taxNumber` cannot be updated (immutable after creation).

---

## PUT /api/companies/{companyId}/logo

Upload or update company logo.

**Authorization**: Bearer JWT (AdminRecruiter belonging to this company)

**Request**: `multipart/form-data`
| Field | Type | Constraints |
|-------|------|-------------|
| file | IFormFile | Required, image (JPG/PNG), ≤5MB |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ logoPath }` | Updated |
| 400 | Error | Invalid file type |
| 403 | Error | Not admin of this company |

---

## POST /api/companies/{companyId}/recruiters

Add a recruiter to the company.

**Authorization**: Bearer JWT (AdminRecruiter belonging to this company)

**Request Body**:
```json
{
  "userId": "string (required, existing recruiter user ID)",
  "recruiterRole": "int? (default: 1=Standard)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ userId, companyId, recruiterRole }` | Added |
| 400 | Error | User is not a recruiter type |
| 403 | Error | Not admin of this company |
| 404 | Error | User not found |
| 409 | Error | Recruiter already belongs to a company |

---

## GET /api/companies/{companyId}/recruiters

List recruiters in a company.

**Authorization**: Bearer JWT (Recruiter belonging to this company)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `[{ userId, firstName, lastName, email, recruiterRole }]` | Success |
| 403 | Error | Not a recruiter of this company |

---

## GET /api/companies/{companyId}/jobs

Get published jobs for a company (public).

**Authorization**: None (public endpoint)

**Query Parameters**: `page`, `pageSize`

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<JobListDto>` | Success |
| 404 | Error | Company not found |

---

## GET /api/companies/dashboard

Get company dashboard data.

**Authorization**: Bearer JWT (Recruiter)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `CompanyDashboardDto` | Success |

**CompanyDashboardDto**:
```json
{
  "totalApplicants": "int",
  "activeJobs": "int",
  "hireRate": "decimal (percentage)",
  "applicationTrends": [{ "week": "string", "count": "int" }],
  "recentJobPosts": ["{ id, title, applicantsCount, createdAt }"],
  "actionRequired": ["{ type, description, targetId }"]
}
```

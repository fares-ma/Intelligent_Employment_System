# API Contract: Companies

**Base Path**: `/api/companies`  
**Authentication**: Mixed (public read, authenticated write)

---

## ~~POST /api/companies~~ (REMOVED)

> **Removed**: Companies are now created via `POST /api/auth/register/company` (see auth.md).
> There is no super-admin role. The first recruiter who creates a company automatically becomes its Admin.

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

**Authorization**: Bearer JWT (Admin Recruiter belonging to this company)
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

**Authorization**: Bearer JWT (Admin Recruiter belonging to this company)

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

## POST /api/companies/{companyId}/invite-codes

Generate a new invite code for the company.

**Authorization**: Bearer JWT (Admin Recruiter belonging to this company)

**Request Body**:
```json
{
  "maxUses": "int (required, 1-100)",
  "expiresAt": "string (required, ISO 8601, must be future)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ id, code, maxUses, currentUses, expiresAt, isActive, createdAt }` | Created |
| 400 | Error | Validation error |
| 403 | Error | Not admin of this company |

**Notes**:
- Code is auto-generated: 6-character alphanumeric (uppercase), unique
- New invite codes start with IsActive=true, CurrentUses=0

---

## GET /api/companies/{companyId}/invite-codes

List all invite codes for the company.

**Authorization**: Bearer JWT (Admin Recruiter belonging to this company)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `[{ id, code, maxUses, currentUses, expiresAt, isActive, createdAt }]` | Success |
| 403 | Error | Not admin of this company |

---

## DELETE /api/companies/{companyId}/invite-codes/{codeId}

Deactivate an invite code.

**Authorization**: Bearer JWT (Admin Recruiter belonging to this company)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Deactivated (sets IsActive=false) |
| 403 | Error | Not admin of this company |
| 404 | Error | Code not found |

**Notes**: Does not hard-delete; sets IsActive=false so existing usage history is preserved.

---

## PUT /api/companies/{companyId}/transfer-admin

Transfer Admin role to another recruiter in the same company.

**Authorization**: Bearer JWT (Admin Recruiter belonging to this company)

**Request Body**:
```json
{
  "newAdminUserId": "string (required, must be a Standard Recruiter in same company)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ previousAdminId, newAdminId }` | Transferred |
| 400 | Error | Target user is not a Standard Recruiter in this company |
| 403 | Error | Not admin of this company |
| 404 | Error | Target user not found |

**Notes**:
- The current Admin is demoted to Standard
- The target user is promoted to Admin
- Both changes happen in a single transaction
- No confirmation step required

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

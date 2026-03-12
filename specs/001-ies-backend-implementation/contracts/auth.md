# API Contract: Authentication

**Base Path**: `/api/auth`  
**Authentication**: Public (registration, login, forgot-password) / Bearer JWT (logout, change-password)

---

## POST /api/auth/register

Register a new candidate.

**Request Body**:
```json
{
  "firstName": "string (required, max 100)",
  "lastName": "string (required, max 100)",
  "email": "string (required, valid email)",
  "password": "string (required, min 8, uppercase+lowercase+digit+special)",
  "phoneNumber": "string? (max 20)",
  "gender": "int (required, 0=Male, 1=Female)",
  "dateOfBirth": "string? (ISO 8601 date)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ token, expiresAt, userId, email, role: "Candidate" }` | Candidate created |
| 400 | `{ statusCode, message, details }` | Validation error |
| 409 | `{ statusCode, message }` | Email already registered |

**Notes**:
- Creates a CandidateUser only
- Returns JWT token immediately (auto-login on registration)
- Password is never returned in any response

---

## POST /api/auth/register/company

Register a new recruiter with a new company. The recruiter becomes Admin automatically.

**Request Body**:
```json
{
  "firstName": "string (required, max 100)",
  "lastName": "string (required, max 100)",
  "email": "string (required, valid email)",
  "password": "string (required, min 8, uppercase+lowercase+digit+special)",
  "phoneNumber": "string? (max 20)",
  "gender": "int (required, 0=Male, 1=Female)",
  "dateOfBirth": "string? (ISO 8601 date)",
  "companyName": "string (required, max 200)",
  "taxNumber": "string (required, unique, max 50)",
  "industry": "string? (max 100)",
  "website": "string? (max 500)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ token, expiresAt, userId, email, role: "Recruiter", companyId }` | Company + Admin Recruiter created |
| 400 | `{ statusCode, message, details }` | Validation error |
| 409 | `{ statusCode, message }` | Email or TaxNumber already exists |

**Notes**:
- Creates both Company and Recruiter in a single transaction
- Recruiter gets Admin role automatically (first recruiter = Admin)
- TaxNumber must be unique across all companies

---

## POST /api/auth/register/recruiter

Register a new recruiter using a company invite code. Gets Standard role.

**Request Body**:
```json
{
  "firstName": "string (required, max 100)",
  "lastName": "string (required, max 100)",
  "email": "string (required, valid email)",
  "password": "string (required, min 8, uppercase+lowercase+digit+special)",
  "phoneNumber": "string? (max 20)",
  "gender": "int (required, 0=Male, 1=Female)",
  "dateOfBirth": "string? (ISO 8601 date)",
  "inviteCode": "string (required, 6-char alphanumeric)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ token, expiresAt, userId, email, role: "Recruiter", companyId }` | Standard Recruiter created |
| 400 | `{ statusCode, message }` | Invalid, expired, or fully-used invite code |
| 409 | `{ statusCode, message }` | Email already registered |

**Notes**:
- Validates invite code: must exist, IsActive=true, CurrentUses < MaxUses, ExpiresAt > now
- Increments CurrentUses on the invite code after successful registration
- Recruiter gets Standard role (not Admin)

---

## POST /api/auth/login

Authenticate a user and return a JWT token.

**Request Body**:
```json
{
  "email": "string (required)",
  "password": "string (required)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ token, expiresAt, userId, email, role, userType }` | Credentials valid |
| 401 | `{ statusCode, message }` | Invalid credentials (generic — no field hint) |

**Notes**:
- Token is JWT with 24h expiry
- `role` is one of: Candidate, Recruiter (includes both Admin and Standard)
- For recruiters, the response also includes `companyId` and `recruiterRole` (Admin/Standard)

---

## POST /api/auth/logout

Invalidate the current session token.

**Authorization**: Bearer JWT (any authenticated user)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 204 | *(no body)* | Token blacklisted |
| 401 | `{ statusCode, message }` | Not authenticated |

**Notes**:
- Adds the token's `jti` to in-memory blacklist with TTL = remaining token lifetime

---

## POST /api/auth/forgot-password

Initiate password reset flow.

**Request Body**:
```json
{
  "email": "string (required)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ message: "If an account exists, a reset link has been sent." }` | Always (no info leak) |

---

## POST /api/auth/reset-password

Complete password reset with token.

**Request Body**:
```json
{
  "email": "string (required)",
  "token": "string (required, from email link)",
  "newPassword": "string (required, same rules as registration)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ message }` | Password reset |
| 400 | `{ statusCode, message }` | Invalid/expired token |

---

## POST /api/auth/change-password

Change password for authenticated user.

**Authorization**: Bearer JWT (any authenticated user)

**Request Body**:
```json
{
  "currentPassword": "string (required)",
  "newPassword": "string (required)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ message }` | Password changed |
| 400 | `{ statusCode, message }` | Current password incorrect or new password invalid |
| 401 | `{ statusCode, message }` | Not authenticated |

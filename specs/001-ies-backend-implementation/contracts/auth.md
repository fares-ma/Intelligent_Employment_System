# API Contract: Authentication

**Base Path**: `/api/auth`  
**Authentication**: Public (registration, login, forgot-password) / Bearer JWT (logout, change-password)

---

## POST /api/auth/register

Register a new user (candidate or recruiter).

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
  "userType": "string (required, 'Candidate' or 'Recruiter')",
  "companyId": "int? (required if userType=Recruiter)"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 | `{ userId, email, userType }` | Account created |
| 400 | `{ statusCode, message, details }` | Validation error / invalid companyId |
| 409 | `{ statusCode, message }` | Email already registered |

**Notes**:
- Recruiter must provide a valid `companyId` for an existing company
- First recruiter for a company is assigned `Admin` role; subsequent recruiters get `Standard`
- Password is never returned in any response

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
- `role` is one of: Candidate, Recruiter, AdminRecruiter
- Error message must NOT reveal whether email or password is wrong

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

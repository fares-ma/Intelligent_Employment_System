# API Contract: Notifications

**Base Path**: `/api/notifications`  
**Authentication**: Bearer JWT

---

## GET /api/notifications

Get notifications for the current user.

**Authorization**: Bearer JWT (any authenticated user)

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| isRead | bool? | | Filter by read status |
| page | int | 1 | |
| pageSize | int | 20 | Max 50 |

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `PagedResult<NotificationDto>` | Success |

**NotificationDto**:
```json
{
  "id": "int",
  "type": "string",
  "title": "string",
  "message": "string",
  "targetUrl": "string?",
  "isRead": "bool",
  "createdAt": "string"
}
```

---

## GET /api/notifications/unread-count

Get count of unread notifications.

**Authorization**: Bearer JWT (any authenticated user)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ count: int }` | Success |

---

## PATCH /api/notifications/{notificationId}/read

Mark a notification as read.

**Authorization**: Bearer JWT (must own the notification)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ id, isRead: true }` | Marked |
| 404 | Error | Not found or not owned |

---

## PATCH /api/notifications/read-all

Mark all notifications as read.

**Authorization**: Bearer JWT (any authenticated user)

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 | `{ updatedCount: int }` | All marked |

---

## Notification Types & Triggers

| Event | Recipients | Channel | Trigger |
|-------|-----------|---------|---------|
| Application submitted | Recruiter(s) of the company | In-app, Email | Candidate applies to a job |
| Application status changed | Candidate | In-app, Email | Recruiter moves application through pipeline |
| Interview scheduled | Candidate + Recruiter | In-app, Email | Recruiter creates an interview |
| Interview reminder | Candidate + Recruiter | Email | 24h and 1h before scheduled time |
| Assessment assigned | Candidate | In-app, Email | Application moves to Assessment stage |
| Candidate accepted | Candidate | In-app, Email | Application status → Accepted |
| Candidate rejected | Candidate | In-app, Email | Application status → Rejected |
| AI score ready | Recruiter(s) | In-app | AI scoring completes for an application |

---

## SignalR Hub: /hubs/notifications

Real-time in-app notification delivery.

**Authentication**: JWT via `access_token` query parameter

**Server → Client Methods**:
| Method | Parameters | Description |
|--------|-----------|-------------|
| ReceiveNotification | `NotificationDto` | New notification pushed to connected user |
| UpdateUnreadCount | `count: int` | Updated unread count after new notification |

**Connection Behavior**:
- User joins their personal group on connection (keyed by userId)
- Notifications are pushed only to the target user's group
- If user is not connected, notification is stored in database and delivered on next fetch

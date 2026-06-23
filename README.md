# Ezkiwi — Virtual Queue API

Backend for the **Ezkiwi** system, a virtual queue management platform. This API provides authentication, queue and ticket CRUD, real-time communication via SignalR, payment processing with Stripe, and transactional email delivery.

> The frontend project (React + TypeScript) is available at [virtual-queue-main](../../../EzkiwiFront/ezkiwi/virtual-queue-main) and consumes this API.

---

## Purpose

Ezkiwi allows businesses to create digital queues to organize customer service without paper or crowding. Customers follow the queue in real time through a public display accessed via QR Code. Attendants call tickets through the management panel and updates reach all connected devices instantly via WebSocket.

**Main flow:**
1. A business registers — a `Company` is created and linked to the owner `User`.
2. The user creates queues (`Queue`), each receiving a unique identifier.
3. On every ticket call, a `PasswordHistory` record is saved and a SignalR event is fired.
4. The paid plan (managed via Stripe) removes queue and call limits.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8 / ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (Code-First) |
| Database | SQL Server (LocalDB in development) |
| Real-time | ASP.NET Core SignalR |
| Authentication | JWT Bearer (HS256) + Refresh Tokens |
| Password hashing | BCrypt.Net |
| Object mapping | AutoMapper |
| Payments | Stripe.net |
| Email | Brevo (Sendinblue) SDK |
| Documentation | Swagger / Swashbuckle |
| Rate Limiting | .NET 8 built-in (token bucket) |

---

## Project Structure

```
VirtualQueueApi/
├── Controllers/             # 5 REST controllers
├── Domain/
│   ├── Entities/            # 7 domain entities
│   ├── Models/              # Commands, Results, Queries, Enums
│   ├── Contracts/           # Service and repository interfaces
│   └── Profiles/            # AutoMapper profiles
├── Data/
│   ├── Repositories/        # 7 repositories (Repository pattern)
│   ├── Mapping/             # EF Core entity configurations (Fluent API)
│   ├── UnitOfWork.cs        # Transaction coordination
│   └── ApplicationDbContext.cs
├── Services/                # Business logic (Auth, User)
├── ExternalServices/        # Third-party integrations (Stripe, Brevo)
├── Hubs/                    # QueueHub — SignalR
├── Configuration/           # Startup extension methods
├── Utils/                   # Middleware and utilities
├── Migrations/              # EF Core migrations
└── Program.cs               # Entry point and application composition
```

---

## Endpoints

### Auth — `/api/auth`

| Method | Route | Description |
|---|---|---|
| POST | `/register` | Register user and company |
| POST | `/login` | Login — returns access + refresh token |
| POST | `/refresh` | Renew access token |
| POST | `/logout` | Revoke refresh token |
| POST | `/forgot-password` | Start password recovery flow |
| POST | `/resend-code` | Resend verification code |
| POST | `/validate-code` | Validate verification code |
| POST | `/confirm-reset` | Reset password with validated code |
| PUT | `/update-password` | Update password (authenticated user) |

### Queues — `/api/queues`

| Method | Route | Description |
|---|---|---|
| GET | `/` | List all company queues |
| GET | `/{id}` | Get a specific queue |
| POST | `/` | Create a new queue (limit: 3 on free plan) |
| DELETE | `/{id}` | Delete a queue (requires paid plan) |

### Passwords — `/api/queues/{queueId}/passwords`

| Method | Route | Description |
|---|---|---|
| GET | `/` | Last 10 called tickets |
| GET | `/highest` | Highest ticket called |
| POST | `/` | Call next ticket + SignalR event |
| PUT | `/recall` | Recall last ticket + SignalR event |
| PUT | `/restart` | Restart queue (requires paid plan) + SignalR event |
| DELETE | `/{passwordId}` | Remove a ticket + SignalR event |

### Checkout — `/api/checkout`

| Method | Route | Description |
|---|---|---|
| POST | `/create-session` | Create Stripe checkout session |
| GET | `/session-status` | Query session status |
| POST | `/webhook` | Receive Stripe webhooks |

### Company — `/api/company`

| Method | Route | Description |
|---|---|---|
| GET | `/` | Get company profile |
| PUT | `/` | Update company profile |

---

## Domain Entities

| Entity | Key Fields | Purpose |
|---|---|---|
| `User` | Email, Name, Password (BCrypt), Role, CompanyId | Users with roles (Owner, Manager, Employee) |
| `Company` | Name, Email | Tenant / organization |
| `Queue` | Name, CompanyId | Virtual queue |
| `PasswordHistory` | Value, QueueId | Record of each called ticket |
| `RefreshToken` | TokenHash, JwtId, ExpiresAt, Used | Refresh token tracking (7-day expiry, single-use) |
| `PasswordResetFlow` | CodeHash, Expiration, AttemptsCount, Ip | Recovery flow with IP-based rate limiting |
| `SubscriptionManagement` | SessionId, SubscriptionId, Status, Start/End | Stripe subscription lifecycle |

---

## Authentication

- JWT tokens signed with HS256.
- Claims include: `sub`, `email`, `CompanyId`, `SubscriptionLimit`, `jti`.
- Refresh tokens stored as a hash, single-use, expire in 7 days.
- Password recovery with a temporary code (10-min expiry) and rate limiting (max 5 attempts per 30-min window).

---

## Real-Time with SignalR

The `/queueHub` hub manages groups per queue:

| Hub Method | Description |
|---|---|
| `JoinQueue(queueId)` | Join a queue group |
| `LeaveQueue(queueId)` | Leave a queue group |

| Broadcast Event | Payload | Trigger |
|---|---|---|
| `ReceivePassword` | `NewPasswordResult` | New ticket called |
| `RecallPassword` | `NewPasswordResult` | Ticket recalled |
| `RemovePassword` | `passwordId` | Ticket removed |
| `RestartPassword` | — | Queue restarted |

---

## Plans & Limits

| Feature | Free Plan | Paid Plan |
|---|---|---|
| Number of queues | 3 | Unlimited |
| Calls per queue | 50 | Unlimited |
| Delete queues | No | Yes |
| Restart queue | No | Yes |
| Recall ticket | No | Yes |

The plan status is injected as a `SubscriptionLimit` claim in the JWT and checked directly in the controllers.

---

## Rate Limiting

- Global token bucket: 20 requests per 30 seconds per IP.
- Burst queue: up to 5 concurrent requests.
- Localhost is exempt.
- Returns HTTP 429 when the limit is exceeded.

---

## Setup & Running

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (installed with Visual Studio) or SQL Server

### Configuration

Fill in the fields in `appsettings.json` or via **User Secrets** in development:

```json
{
  "JwtSettings": {
    "Secret": "<key-minimum-32-characters>",
    "Issuer": "ezkiwi",
    "Audience": "ezkiwi-users"
  },
  "StrikeService": {
    "ApiKey": "<stripe-secret-key>",
    "webHookSecret": "<stripe-webhook-secret>"
  },
  "BrevoService": {
    "ApiKey": "<brevo-api-key>"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(LocalDB)\\MSSQLLocalDB;Database=queueGpt;..."
  }
}
```

> Note: the configuration key is named `StrikeService` (original typo in the project).

### Apply migrations and run

```bash
dotnet ef database update
dotnet run
```

Swagger documentation will be available at `https://localhost:7047/swagger` in Development and Local environments.

---

## Architectural Patterns

- **Repository Pattern** — generic repository with LINQ expression support, includes, and pagination.
- **Unit of Work** — transaction coordination across repositories.
- **Service Layer** — business logic decoupled from controllers.
- **AutoMapper** — entity-to-DTO projection.
- **Domain-Driven Design (DDD)** — clear separation between entities, contracts, commands, and results.
- **Global Exception Handler** — middleware maps domain exceptions to standard HTTP responses (`BusinessException` → 400, `UnauthorizedException` → 401, `EntityNotFoundException` → 404).

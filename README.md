# Dog Walking Manager

A desktop application for managing a dog walking business. Clients register their dogs and request walks; walkers see matching requests on a calendar and accept them; admins oversee everything.

Built with **WinForms (.NET 8)** and **Entity Framework Core** on **SQL Server LocalDB**.

---

## What Does It Do?

Imagine you run a dog walking company. You have **clients** (dog owners), **walkers** (your employees), and an **admin** who manages the operation.

- **Clients** sign up, register their dogs, pick a subscription plan, and request walks.
- **Walkers** set their availability (days, hours, zones). The system matches open walk requests to available walkers automatically. Walkers accept or decline from a monthly calendar.
- **Admins** manage clients, dogs, walkers, users, and walk events from a tabbed dashboard.

Each walk goes through a clear lifecycle: **Requested → Proposed → Accepted → In Progress → Completed** (or Cancelled at any point). Subscription plans (Free, Basic, Premium) limit how many walks a client can book per month.

When a walker accepts a walk, the client gets a real-time notification over the local network via UDP multicast (can be changed in the future to a message broker for production).

---

## How to Run

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| SQL Server LocalDB | Included with Visual Studio |
| Visual Studio / Rider | 2022+ recommended |

### Setup

```bash
# 1. Clone the repo
git clone <repo-url>
cd DogWalkingApp

# 2. Restore packages
dotnet restore

# 3. Apply EF Core migrations (creates the database)
dotnet ef database update --project DogWalking.Infrastructure --startup-project DogWalking.Infrastructure

# 4. Run the app
dotnet run --project DogWalking.WinForms

# 5. Run all tests (60 unit + 40 integration)
dotnet test
```

### Default Accounts

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin123!` | Admin |
| `walker1` | `Walker123!` | Walker |

New walker and client accounts can be created from the login screen's registration panel.

---

## Project Structure

```
DogWalkingApp/
├── DogWalking.Domain/           # Entities, enums, interfaces, business rules
│   ├── Entities/                # Client, Dog, User, WalkEvent, WalkerAvailability...
│   ├── Enums/                   # WalkStatus, SubscriptionType, RecurrenceType...
│   ├── Interfaces/              # Repository contracts (IClientRepository, etc.)
│   ├── Services/                # Walk limit strategies (domain logic)
│   ├── ValueObjects/            # PhoneNumber value object
│   └── Exceptions/              # DomainException, ConcurrencyException
│
├── DogWalking.Application/      # Use cases, DTOs, validators, service interfaces
│   ├── Services/                # AuthService, ClientService, WalkEventService...
│   ├── DTOs/                    # Data transfer objects (no EF dependencies)
│   ├── Interfaces/              # Service contracts (IWalkEventService, etc.)
│   └── Validators/              # FluentValidation rules
│
├── DogWalking.Infrastructure/   # EF Core, repositories, DB config, messaging
│   ├── Data/                    # DbContext + entity configurations
│   ├── Repositories/            # Repository implementations
│   ├── Messaging/               # UDP multicast notification service
│   └── Extensions/              # DI registration (ServiceCollectionExtensions)
│
├── DogWalking.WinForms/         # UI layer (forms, controls, DI setup)
│   ├── Forms/                   # LoginForm, MainForm, DogDialog, WalkEventForm...
│   ├── Controls/                # WalkCalendarPanel, ToastNotification
│   └── Program.cs               # Entry point + DI container setup
│
├── DogWalking.Tests/            # Unit tests (xUnit + Moq)
│   ├── Domain/                  # Entity logic, value objects, strategies
│   └── Services/                # Service-layer tests with mocked repos
│
└── DogWalking.IntegrationTests/ # Integration tests (SQLite in-memory)
    └── Services/                # Full-stack tests: service → repo → DB
```

---

## Architecture

The solution follows **Clean Architecture** with four layers. Dependencies only point inward: UI → Application → Domain. Infrastructure implements Domain interfaces.

```
┌──────────────────────────────────┐
│          WinForms (UI)           │  Thin forms, no business logic
├──────────────────────────────────┤
│    Application (Use Cases)       │  Services, DTOs, validation
├──────────────────────────────────┤
│     Domain (Business Rules)      │  Entities, enums, interfaces
├──────────────────────────────────┤
│   Infrastructure (Persistence)   │  EF Core, repositories, messaging
└──────────────────────────────────┘
```

**Why this structure?**

- Business logic lives in **Domain entities** (e.g., `WalkEvent.TransitionTo()` enforces valid status changes). Forms never decide business rules.
- The **Application layer** coordinates use cases without knowing how data is stored. It depends on repository *interfaces*, not EF Core.
- **Infrastructure** provides the implementations: EF Core repositories, database configurations, the notification service.
- The **WinForms layer** only handles user interaction: displaying data, capturing input, calling service methods.

This means you can swap the database, change the UI framework, or replace the notification mechanism without touching business logic.

---

## Design Decisions

### Domain-Driven Design

Entities are **rich**, not anemic. `WalkEvent` has a state machine that validates transitions (`Requested → Proposed → Accepted → ...`). `Dog.ValidateNoConflictingWalk()` prevents overlapping walks. `Client` tracks subscription and zone. All invariants are enforced at the domain level.

### Design Patterns Used

| Pattern | Where | Why |
|---------|-------|-----|
| **Repository** | `IClientRepository`, `IDogRepository`, etc. | Decouples domain from EF Core. Application depends on abstractions. |
| **Unit of Work** | `UnitOfWork` class | All repositories share one `DbContext` instance → single transaction boundary. |
| **Strategy** | `WalkLimitStrategyFactory` + per-tier strategies | Each subscription tier (Free/Basic/Premium) has its own walk limit rules. Adding a new tier means adding one class, no `if/else` chains. |
| **Factory** | `WalkLimitStrategyFactory.Create()` | Encapsulates strategy selection. The caller doesn't know which concrete strategy it gets. |
| **Dependency Injection** | `ServiceCollectionExtensions`, `Program.cs` | All services are registered in the DI container. Forms and services receive their dependencies through constructors. |
| **Singleton** | `INotificationService` (UDP multicast) | One socket listener per application instance, shared across all scopes. |

### Entity Framework Core

- **Code First** with Fluent API configurations (one per entity in `Data/Configurations/`).
- **Async everywhere** — all repository methods are async. UI never blocks.
- **AsNoTracking** on read-only queries for better performance.
- **Optimistic concurrency** via `RowVersion` on `WalkEvent` — prevents lost updates when two users modify the same walk.
- **Composite indexes** on `(DogId, WalkDate, Status)` and `(Status, WalkDate)` for the most frequent query patterns.
- **Migrations** managed with `dotnet ef migrations`.

### Validation

Two layers of validation:

1. **FluentValidation** at the Application layer — checks input format, required fields, date ranges before touching the database.
2. **Domain validation** in entities — enforces business invariants (no overlapping walks, valid status transitions, subscription limits).

Validation errors surface to the UI through exception messages displayed in labels (no raw stack traces).

### Error Handling

- **Domain exceptions** (`DomainException`, `ConcurrencyException`) carry meaningful messages for the user.
- Services catch EF `DbUpdateConcurrencyException` in `UnitOfWork.CommitAsync()` and wrap it in a domain-friendly `ConcurrencyException`.
- UI forms wrap all async calls in try/catch, showing errors in labels or message boxes.
- The notification system is **best-effort** — a network failure never blocks a walk operation.

### Caching

`IMemoryCache` is used at the repository level for data that changes infrequently:

- **Client profiles** (subscription lookups) — 5 min TTL, invalidated on write.
- **Walker availability slots** — 5 min TTL, invalidated on write.

This follows the **cache-aside** pattern: check cache first, fall back to database, store result. Write operations explicitly evict the cache entry to prevent stale data.

### Real-Time Notifications

Walkers accepting a walk triggers a UDP multicast message on the local network. All app instances subscribed to the multicast group receive it. Clients see a toast notification when their walk gets accepted.

This is a lightweight solution for LAN environments. For production at scale, it would be replaced with a message broker (RabbitMQ, Azure Service Bus).

### Pagination

The admin Walk Events tab uses **server-side pagination** (10 items per page). The repository uses `Skip/Take` with `CountAsync` to avoid loading all rows. A `PagedResultDto<T>` carries items, total count, and page metadata.

---

## Testing Strategy

**100 tests total**: 60 unit + 40 integration.

### Unit Tests (`DogWalking.Tests`)

- **Entity tests**: status transitions, validation rules, edge cases (e.g., transitioning to an invalid status throws `DomainException`).
- **Value object tests**: `PhoneNumber` parsing, equality, edge cases.
- **Strategy tests**: walk limits per subscription tier, daily limits, boundary cases.
- **Service tests (mocked)**: Moq-based tests verifying service orchestration without hitting a database.

### Integration Tests (`DogWalking.IntegrationTests`)

- **SQLite in-memory** database — each test gets a fresh database, no cleanup needed.
- Full service → repository → database round-trips.
- Cover all six services: Auth, Client, Dog, User, WalkEvent, WalkerAvailability.
- Test real EF Core behavior: relationships, cascades, concurrency.

### What's tested

- Valid and invalid status transitions
- Subscription limit enforcement (walks per month, walks per day)
- Walk scheduling with recurrence
- Walk claiming/declining workflow
- Authentication (login, registration, duplicate username detection)
- CRUD operations with validation
- Concurrency conflict handling

---

## AI Usage

AI was used as a development aid, not a replacement:

- **UI layout**: Assisted with WinForms control positioning and Designer code (repetitive boilerplate).
- **Test generation**: Helped scaffold test structures, which were then reviewed and adjusted to cover the right edge cases.
- **Code comments**: Assisted with XML documentation on interfaces and complex methods.

All AI output was reviewed, tested, and adapted to fit the project's architecture. The domain model, business rules, architecture decisions, and overall design are my own.

---

## Future Improvements

If this were a production system with more time, I would consider:

- **Authentication**: Replace SHA-256 hashing with bcrypt/Argon2 and add token-based session management.
- **CQRS**: Separate read models from write models. The admin dashboard could use optimized read projections (e.g., denormalized views for walk listings) while writes go through the domain model.
- **Message broker**: Replace UDP multicast with RabbitMQ or Azure Service Bus for reliable cross-network messaging, delivery guarantees, and message persistence.
- **Distributed caching**: Move from in-process `IMemoryCache` to Redis for multi-instance deployments.
- **Reporting**: Monthly/weekly reports for clients and walkers (walks completed, revenue, walker performance).
- **Geolocation**: Replace zone-based matching with GPS coordinates and radius-based availability.
- **Mobile client**: Expose services as a REST API; build a companion mobile app for walkers on the go.
- **Audit log**: Track who changed what and when, especially for walk status transitions and cancellations.
- **Rate limiting / throttling**: Protect against abuse in a web-exposed scenario.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 12 / .NET 8 |
| UI | Windows Forms |
| ORM | Entity Framework Core 8 |
| Database | SQL Server LocalDB |
| Validation | FluentValidation |
| Testing | xUnit, Moq |
| Integration Tests | SQLite in-memory |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Caching | Microsoft.Extensions.Caching.Memory |
| Notifications | UDP Multicast (System.Net.Sockets) |

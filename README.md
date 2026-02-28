# Dog Walking Manager

A desktop application for managing a dog walking business. Clients register their dogs and request walks, walkers see matching requests on a calendar and accept them, admins oversee everything. Built with WinForms (.NET 10) and Entity Framework Core on SQL Server LocalDB.

## What Does It Do?

Clients sign up, register their dogs, pick a subscription plan, and request walks. They can optionally choose a preferred walker, schedule recurring walks, or request walks via natural language through an AI assistant.

Walkers set their availability (days, hours, zones). The system matches open requests to available walkers automatically. Walkers accept or decline from a monthly calendar.

Admins manage clients, dogs, walkers, users, and walk events from a tabbed dashboard. They can create walks for any client regardless of subscription limits.

Each walk follows a lifecycle: Requested, Proposed, Accepted, InProgress, Completed (or Cancelled at any point). Subscription plans (Free, Basic, Pro, Premium) limit how many walks a client can book per month. When a walker accepts a walk, the client gets a real-time notification over the local network via UDP multicast.

## How to Run

You need .NET SDK 10.0+, SQL Server LocalDB (included with Visual Studio), and Visual Studio 2022+ is recommended.

```bash
git clone https://github.com/natafridman/DogWalkingApp.git
cd DogWalkingApp
dotnet restore
dotnet ef database update --project DogWalking.Infrastructure --startup-project DogWalking.Infrastructure
dotnet run --project DogWalking.WinForms
dotnet test
```

Default accounts: `admin` / `Admin123!` (Admin role), `walker1` / `Walker123!` (Walker role). New walker and client accounts can be created from the login screen's registration panel.

## Project Structure

```
DogWalkingApp/
├── DogWalking.Domain/           # Entities, enums, interfaces, business rules
├── DogWalking.Application/      # Use cases, DTOs, validators, service interfaces
├── DogWalking.Infrastructure/   # EF Core, repositories, DB config, messaging
├── DogWalking.WinForms/         # UI layer (forms, controls, DI setup)
├── DogWalking.Tests/            # Unit tests (xUnit + Moq)
└── DogWalking.IntegrationTests/ # Integration tests (SQLite in-memory)
```

## Architecture

I chose Clean Architecture with four layers. Dependencies only point inward: UI, Application, Domain, Infrastructure.

```
┌──────────────────────────────────┐
│          WinForms (UI)           │
├──────────────────────────────────┤
│    Application (Use Cases)       │
├──────────────────────────────────┤
│     Domain (Business Rules)      │
├──────────────────────────────────┤
│   Infrastructure (Persistence)   │
└──────────────────────────────────┘
```

I made this decision because business rules change at a different pace than the UI or storage. By isolating the domain at the center, I can swap the database, the UI framework, or the notification mechanism without touching walk lifecycle rules or subscription logic. Repository interfaces live in Domain, implementations in Infrastructure, so Application never knows about EF Core. This also makes everything testable with mocked repositories.

## Design Decisions

### Rich Domain Model (DDD)

Entities are rich, not anemic. `WalkEvent` has a state machine that validates transitions, `Dog` prevents overlapping walks, `Client` tracks subscription and zone. I decided to put the rules on the entities themselves because invariants belong where the data lives. If transition logic lived in a service, someone could forget to validate. With `TransitionTo()` and `ProposeToWalker()` on the entity, the rules are impossible to bypass.

If I had gone with anemic entities, the alternative would be domain services: a `WalkEventDomainService` that receives the entity as a parameter and runs the validations before mutating it. The entity would be a plain data container (just properties, no logic), and every service that needs to change status would have to call the domain service first. The problem is that nothing forces you to go through that service, so a new developer (or even myself in a hurry) could just set `walk.Status = WalkStatus.Completed` directly and skip all the guards. With rich entities, the setter is private and the only way to change state is through the method that validates.

### Strategy Pattern for Subscription Limits

Each subscription tier has its own `IWalkLimitStrategy` implementation, created by `WalkLimitStrategyFactory`. I thought about a simple if/else chain, but subscription tiers will grow. With the Strategy Pattern, adding a new tier is one class plus one line in the factory. Each strategy encapsulates max walks per month, daily limits, and a description all in one place.

### Walk Lifecycle State Machine

```
Requested → Proposed → Accepted → InProgress → Completed
```

Any state can transition to Cancelled. Rejected loops back to Requested so the walk re-enters the pool for another walker. The domain records each decline in a `WalkDecline` entity so the same walker won't see it again. I implemented this directly in the entity because invalid transitions corrupt data, and I wanted it to be impossible for the UI to skip a step.

### Partial Classes for WalkEventService

`WalkEventService` is split into four files: `.cs` (constructor, helpers), `.Admin.cs`, `.Walker.cs`, `.Client.cs`. I considered separate service classes, but they all share the same `IUnitOfWork`, logger, and validator. Partial classes give me organizational separation without duplicating constructors or complicating DI.

### Modular DI Registration

Infrastructure services register through focused methods: `AddDatabase()`, `AddServices()`, `AddCaching()`, `AddNotifications()`, `AddAI()`. Originally I had one big method, but I split it because not every host needs everything. The API doesn't need notifications, the test harness only needs database and services.

### STA Thread and Synchronous Main

`Program.Main()` is synchronous (`void Main`, not `async Task Main`). I originally had it async, but I ran into `ThreadStateException` with ComboBox autocomplete. WinForms requires STA for OLE controls, and after an await, continuations can resume on an MTA thread. Synchronous main guarantees everything initializes on the STA thread.

### DbContext Concurrency Protection

All DB operations in `MainForm` are serialized through a `SemaphoreSlim(1,1)` plus a `_ready` flag. This was a problem that took me a while. EF Core's DbContext is not thread-safe, and in WinForms, tab Enter events fire during `TabPages.AddRange()` and again when the form gains focus, creating overlapping async handlers on the same context. The semaphore alone wasn't enough because the events fire before the session is initialized. The `_ready` flag ignores those spurious events, and the real load happens once when the form gets focus. I also cancel the `CancellationTokenSource` on logout so in-flight operations from the previous session don't collide with the new one.

### Preferred Walker Selection

Clients can optionally choose a specific walker from a dropdown. I made it optional because most clients just want someone available. When a preferred walker is chosen, the walk skips Requested and goes directly to Proposed for that walker, reusing the existing propose/accept workflow.

### Recurring Walks

Clients can schedule walks with four patterns: one-time, all working days, every two working days, or same day of the week. I thought about lazy expansion like Google Calendar, but I generate all instances upfront because each walk needs its own status tracking and cancellation capability. Individual entities make each walk independent.

### Repository + Unit of Work

All repositories share a single DbContext through UnitOfWork. This gives me one transaction boundary across multiple repositories. When scheduling a recurring walk, the service reads dogs, checks clients, queries events, and inserts new ones, all in one `CommitAsync()`. If anything fails, nothing persists.

### Two-Layer Validation

FluentValidation at the Application layer catches malformed input early with user-friendly messages. Domain validation in entities enforces business invariants that need data context, like "this dog already has a walk at that time." Separating them keeps each layer focused on what it does best.

### Real-Time Notifications via UDP Multicast

When a walker accepts a walk, a UDP multicast message goes out on the local network. I chose this because it's zero-infrastructure for a LAN desktop app, no broker to install. The `INotificationService` interface is abstract enough to swap for RabbitMQ later.

### Caching

`IMemoryCache` at the repository level for data that changes infrequently: client profiles and walker availability slots, both with 5 minute TTL. Cache-aside pattern with explicit invalidation on writes.

### Server-Side Pagination

The admin's Walk Events tab uses server-side pagination with `Skip/Take`. An active business could have thousands of events, so loading them all would be slow. `PagedResultDto<T>` keeps memory usage constant.

### Logging

Serilog with two sinks: console and rolling file (logs/ folder, one file per day, keeps the last 7). I chose Serilog because the built-in provider has no file sink. EF Core internals are filtered to Warning so the output stays clean. Configuration lives in appsettings.json so I can change log levels without recompiling.

To improve performance I would also use logging instrumentation (OpenTelemetry) instead of writing log strings everywhere. Instrumentation emits structured traces and metrics with minimal overhead, and lets you correlate requests across services without manually adding context to each log call. For a larger system I would stream those logs into Snowflake. File-based logging doesn't scale when you need to query months of audit data across services, and Snowflake handles that with standard SQL without impacting the main database.

### Docker Support

The API has a Dockerfile and the solution root has a docker-compose.yml that runs SQL Server 2022 and the API. The connection string is overridden via environment variable. Multi-stage Dockerfile keeps the final image small.

```bash
docker-compose up --build
docker-compose down
```

## Design Patterns

| Pattern | Where | Why |
|---------|-------|-----|
| Repository | `IClientRepository`, `IDogRepository`, etc. | Decouples domain from EF Core |
| Unit of Work | `UnitOfWork` class | Single transaction boundary across repositories |
| Strategy | `WalkLimitStrategyFactory` + per-tier strategies | New tiers = new class, zero changes to existing code |
| Factory | `WalkLimitStrategyFactory.Create()` | Encapsulates strategy selection |
| State Machine | `WalkEvent.TransitionTo()` | Enforces valid transitions at the domain level |
| Partial Classes | `WalkEventService.{Admin,Walker,Client}.cs` | Organizational separation by role |
| Dependency Injection | `ServiceCollectionExtensions`, `Program.cs` | Constructor injection everywhere |
| Singleton | `INotificationService` (UDP multicast) | One socket listener per app instance |
| Cache-Aside | `IMemoryCache` at repository level | Check cache, fallback to DB, invalidate on writes |

## Testing

100 tests total: 60 unit + 40 integration.

Unit tests cover entity state transitions, value objects, subscription strategies, and service orchestration with mocked repositories using Moq.

Integration tests use SQLite in-memory so each test gets a fresh database in microseconds, no SQL Server required. They cover full round-trips (service, repository, database) for all six services: Auth, Client, Dog, User, WalkEvent, WalkerAvailability.

What's tested: valid and invalid state transitions, subscription limit enforcement, recurring walk scheduling, walk claiming/declining/unaccepting workflow, preferred walker flow, authentication, CRUD with validation, concurrency conflicts, admin subscription bypass.

I chose SQLite in-memory because it gives me real EF Core behavior without requiring a SQL Server instance, and there's no shared state between tests.

## AI Walk Request Feature

Clients can request walks using natural language (e.g., "walk Rocky tomorrow at 3pm in Palermo for 45 minutes"). The text goes to OpenAI which extracts structured fields. I added a confirmation step because AI parsing is probabilistic, the user verifies the extracted data before submitting.

## Error Handling

Domain exceptions carry meaningful messages, not stack traces. `UnitOfWork.CommitAsync()` wraps EF's concurrency exceptions into domain-friendly ones. UI forms wrap all async calls in try/catch. Notifications are best-effort so network failures never block walk operations. FluentValidation surfaces clean error messages.

I considered a global error handler, but WinForms async void handlers don't propagate to a central handler. The `RunAsync()` helper in MainForm wraps the common pattern (semaphore + try/catch + error display) so individual handlers stay clean.

In a larger system I wouldn't throw as many exceptions. Exceptions are expensive in terms of performance, so I would only keep the ones that need to bubble up to trigger a retry or a rollback. For validation and expected failures I would use a Result pattern instead, returning success/failure without the overhead of throwing.

## Future Improvements

If this were a production system I would consider: replacing SHA-256 with bcrypt/Argon2, adding CQRS with read projections for the admin dashboard, swapping UDP for RabbitMQ or Azure Service Bus, moving to Redis for distributed caching, adding monthly reports, replacing zone-based matching with GPS coordinates, exposing services as a REST API for a mobile companion app, adding an audit log for state transitions, and rate limiting for web-exposed scenarios.

I would also add a rating system for walkers and clients. Clients could rate their walker after each walk, and that score would be visible when choosing a preferred walker, so the client can decide based on what other people say about that walker. Right now the decision of accepting a walk is entirely on the walker and they have to figure out the logistics on their own. I would integrate maps with suggested routes near parks and quiet zones, showing estimated pickup and dropoff times for each house, so walkers can plan their day better before accepting.

Another thing I would add is a dog profile with more detail: temperament (friendly, aggressive, shy), whether it's neutered, sex or size. This matters because a walker needs to know what they're getting into before accepting, especially if they're walking multiple dogs at once. Two aggressive unneutered males together is a bad idea, and right now the walker has no way to know that from the app.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 13 / .NET 10 |
| UI | Windows Forms |
| ORM | Entity Framework Core 10 |
| Database | SQL Server LocalDB |
| Validation | FluentValidation |
| Logging | Serilog (Console + File sinks) |
| Testing | xUnit, Moq |
| Integration Tests | SQLite in-memory |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Caching | Microsoft.Extensions.Caching.Memory |
| Notifications | UDP Multicast (System.Net.Sockets) |
| AI | OpenAI API |
| Containerization | Docker + docker-compose |

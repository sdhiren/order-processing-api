# Order Processing API

A RESTful Order Processing API built with **ASP.NET Core (.NET 10)**, following Clean Architecture principles. It manages the full lifecycle of orders — creation, status updates, cancellation, and automated background processing — backed by a **PostgreSQL** database.

---

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Make Commands](#make-commands)
- [API Endpoints](#api-endpoints)
- [Order Status Flow](#order-status-flow)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Running Tests](#running-tests)
- [Development Notes](#development-notes)

---

## Architecture

The solution uses **Clean Architecture** with four layers, each as its own project:

```
┌─────────────────────────────────────────────────────────┐
│                    API (Presentation)                   │
│          Controllers · Middleware · Swagger/OpenAPI     │
├─────────────────────────────────────────────────────────┤
│                      Application                        │
│        Services · DTOs · Validators · Interfaces        │
├─────────────────────────────────────────────────────────┤
│                       Domain                            │
│           Entities · Enums · Domain Exceptions          │
├─────────────────────────────────────────────────────────┤
│                    Infrastructure                       │
│    EF Core · PostgreSQL · Repository · Background Jobs  │
└─────────────────────────────────────────────────────────┘
```

Dependency direction: **API → Application → Domain ← Infrastructure**

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Database | PostgreSQL 17 |
| ORM | Entity Framework Core 10 (Npgsql) |
| Validation | FluentValidation 11 |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Background Jobs | `BackgroundService` (hosted service) |
| Unit Tests | xUnit · Moq · FluentAssertions |
| Integration Tests | xUnit · Testcontainers (PostgreSQL) · `WebApplicationFactory` |
| Coverage | coverlet |
| Containerization | Docker · Docker Compose |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) with Compose v2 (`docker compose`)
- `make` (pre-installed on macOS/Linux; on Windows use WSL or [Make for Windows](https://gnuwin32.sourceforge.net/packages/make.htm))
- _(Optional)_ [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) for migration commands:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Quick Start

Start the full stack (API + PostgreSQL) with a single command:

```bash
make up
```

This builds the Docker image and starts both services. The API will:
1. Wait for PostgreSQL to be healthy
2. Automatically apply EF Core database migrations on startup
3. Be available at **http://localhost:5029**

Browse the interactive API docs at **http://localhost:5029/swagger**.

To stop the stack:

```bash
make down
```

---

## Make Commands

### Docker / Stack

| Command | Description |
|---------|-------------|
| `make up` | Build images and start the API + PostgreSQL in Docker |
| `make down` | Stop and remove containers (data volume is preserved) |
| `make down-volumes` | Stop containers **and** delete the postgres data volume |
| `make build` | Re-build the API Docker image |
| `make logs` | Tail logs for all services |
| `make ps` | Show running container status |
| `make db-only` | Start only the PostgreSQL container |

### Local Development

| Command | Description |
|---------|-------------|
| `make restore` | Restore NuGet packages |
| `make run` | Run the API locally (requires a running postgres) |
| `make clean` | Remove all `bin/` and `obj/` build artefacts |

### Database

| Command | Description |
|---------|-------------|
| `make migrate` | Apply EF Core migrations against the local postgres instance |

### Tests

| Command | Description |
|---------|-------------|
| `make test` | Run **all** tests (unit + integration) |
| `make test-unit` | Run unit tests only |
| `make test-integration` | Run integration tests only |
| `make coverage` | Run all tests and collect coverage (Cobertura XML → `./coverage/`) |

---

## API Endpoints

Base URL: `http://localhost:5029`

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/orders` | Create a new order |
| `GET` | `/api/orders` | List all orders (optional `?status=` filter) |
| `GET` | `/api/orders/{id}` | Get a single order by ID |
| `PATCH` | `/api/orders/{id}/status` | Update an order's status |
| `DELETE` | `/api/orders/{id}` | Cancel a pending order |
| `GET` | `/health` | Health check (includes database connectivity) |

### Create Order — `POST /api/orders`

```json
{
  "customerName": "Jane Doe",
  "customerEmail": "jane@example.com",
  "items": [
    {
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 19.99
    }
  ]
}
```

**Response `201 Created`**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "Jane Doe",
  "customerEmail": "jane@example.com",
  "status": 0,
  "statusDisplay": "PENDING",
  "totalAmount": 39.98,
  "createdAt": "2026-05-01T12:00:00Z",
  "updatedAt": "2026-05-01T12:00:00Z",
  "items": [...]
}
```

### Update Order Status — `PATCH /api/orders/{id}/status`

Allowed status transitions via this endpoint: `Processing`, `Shipped`, `Delivered`.

```json
{
  "status": 1
}
```

> To cancel an order use `DELETE /api/orders/{id}` — only `Pending` orders can be cancelled.

---

## Order Status Flow

```
            ┌──────────────────────────────────────┐
            │           Background Job             │
            │        (every 5 minutes)             │
            ▼                                      │
PENDING ──────────► PROCESSING ──► SHIPPED ──► DELIVERED
   │
   └──► CANCELLED  (via DELETE endpoint, only from PENDING)
```

A background `HostedService` automatically advances `Pending` → `Processing` every **5 minutes**.

---

## Project Structure

```
order-processing-api/
├── Dockerfile
├── docker-compose.yml
├── Makefile
├── OrderProcessing.slnx
│
├── src/
│   ├── OrderProcessing.API/            # Presentation layer
│   │   ├── Controllers/                # OrdersController + validation helpers
│   │   ├── Middleware/                 # GlobalExceptionHandler
│   │   └── Program.cs
│   │
│   ├── OrderProcessing.Application/    # Use-case layer
│   │   ├── DTOs/                       # Request/Response records
│   │   ├── Interfaces/                 # IOrderService, IOrderRepository
│   │   ├── Mappings/                   # Entity → DTO extensions
│   │   ├── Services/                   # OrderService
│   │   └── Validators/                 # FluentValidation validators
│   │
│   ├── OrderProcessing.Domain/         # Core domain
│   │   ├── Entities/                   # Order, OrderItem
│   │   ├── Enums/                      # OrderStatus
│   │   └── Exceptions/                 # DomainException hierarchy
│   │
│   └── OrderProcessing.Infrastructure/ # Data & jobs
│       ├── BackgroundJobs/             # OrderStatusUpdateJob
│       └── Persistence/
│           ├── AppDbContext.cs
│           ├── Configurations/         # EF entity configurations
│           ├── Migrations/
│           └── Repositories/           # OrderRepository
│
└── tests/
    ├── OrderProcessing.UnitTests/       # Domain + service unit tests (Moq/xUnit)
    └── OrderProcessing.IntegrationTests/# Full HTTP tests (Testcontainers + WebApplicationFactory)
```

---

## Configuration

Connection string and log levels are configured in `appsettings.json`. For local development, `appsettings.Development.json` is merged on top.

### Environment Variables (Docker)

When running via Docker Compose the API container receives the connection string via an environment variable, which overrides `appsettings.json`:

```
ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=order_processing_db;Username=postgres;Password=yourpassword
```

Any `appsettings.json` value can be overridden the same way using `__` as a separator for nested keys.

### Changing Database Credentials

Edit the values in `docker-compose.yml` under the `db` service environment and update the `ConnectionStrings__DefaultConnection` value for the `api` service accordingly.

---

## Running Tests

### Unit Tests

Unit tests cover domain logic, `OrderService`, and FluentValidation validators. They have no external dependencies.

```bash
make test-unit
```

### Integration Tests

Integration tests spin up a real PostgreSQL database automatically via **Testcontainers** — no manual setup required. Docker must be running.

```bash
make test-integration
```

### All Tests

```bash
make test
```

### Coverage Report

```bash
make coverage
```

Cobertura XML reports are written to `./coverage/`. Use [ReportGenerator](https://github.com/danielpalme/ReportGenerator) to produce an HTML report:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:Html
open ./coverage/report/index.html
```

---

## Development Notes

- **Automatic migrations**: The API applies pending EF Core migrations on startup via `db.Database.MigrateAsync()`. There is no need to run migrations manually in Docker.
- **Optimistic concurrency**: The `Order` entity uses a `xmin` row-version column (`RowVersion`) for optimistic concurrency control.
- **Health check**: `GET /health` checks both the ASP.NET Core host and the database connection, tagged `db` and `postgres`.
- **OpenAPI / Swagger**: Available at `/swagger` when `ASPNETCORE_ENVIRONMENT=Development`.

# RESTful API Training — .NET 10 + MySQL (Minimal APIs)

*A hands-on learning project using Minimal APIs, Entity Framework Core, and a clean layered architecture*

Repository name: **`restful_api_training`**

---

## Table of Contents

1. [Project Overview](#project-overview)  
2. [Current Status](#current-status)  
3. [Learning Goals](#learning-goals)  
4. [Domain Model — Users](#domain-model--users)  
5. [High-Level Architecture](#high-level-architecture)  
6. [Solution & Folder Structure](#solution--folder-structure)  
7. [Technology Stack](#technology-stack)  
8. [REST API Design](#rest-api-design)  
   - [Endpoint Summary](#endpoint-summary)  
   - [Upload Endpoint](#upload-endpoint)  
   - [Error Handling & Status Codes](#error-handling--status-codes)  
9. [DTOs & Request/Response Models](#dtos--requestresponse-models)  
10. [Database Schema](#database-schema)  
11. [REST Constraints & How This Project Demonstrates Them](#rest-constraints--how-this-project-demonstrates-them)  
12. [Non-Functional Requirements (NFRs)](#non-functional-requirements-nfrs)  
13. [Running the Project (Local Dev)](#running-the-project-local-dev)  
14. [Recommended Training Flow](#recommended-training-flow)  
15. [Future Extensions](#future-extensions)  

---

## Project Overview

This repository contains a **training-oriented implementation of a modern RESTful API** using:

- **.NET 10 Minimal APIs**
- **MySQL** as the backing relational database
- **Entity Framework Core** for data access
- A **clean, layered architecture** that can scale to enterprise systems with hundreds of tables

The goal is to keep the **functionality small** (single resource: `users`) while designing the **architecture like a real enterprise application**.  
This allows learners to see how a simple CRUD demo fits into a much larger, production-ready design.

The main resource in this training project is the **`users`** table with:

- Full **CRUD** operations  
- A generic **profile picture upload** endpoint (file stored on disk, URL stored in DB)  

A separate document, **`RESTful.md`**, is included in the repo to teach the **theory of REST, RPC, SOAP, etc.**  
This **`README.md`** focuses on the **code, architecture, and practical walkthrough**.

---

## Current Status

As of now, the project implements:

- ✅ Layered solution: **Domain, Application, Infrastructure, API**
- ✅ EF Core integration with MySQL (via Pomelo provider)
- ✅ Database migrations (`InitialCreate`) and `users` table
- ✅ Seeded `admin` user created directly via DB for testing
- ✅ Minimal APIs for:
  - Create user
  - Read user by ID
  - List users (with pagination)
  - List users (all)
  - Filtered listing (with/without pagination)
  - Update user
  - Delete user
  - Upload profile picture (generic field & folder)
- ✅ Swagger UI + OpenAPI JSON
- ✅ Simple HTML landing page at `/` with links

Planned but **not implemented yet** (for future training sessions):

- ❌ JWT authentication  
- ❌ Role-based authorization (`admin` vs `read_only`)  
- ❌ Auth endpoints (`/api/auth/login`)  
- ❌ Centralised error middleware, ProblemDetails, etc.

For tomorrow’s session, the focus is on:

- The **architecture**
- The **Minimal API endpoints**
- The **EF Core + MySQL integration**
- The **RESTful design** (URLs, verbs, status codes)

---

## Learning Goals

By working through this project, learners should be able to:

- Understand the basics of **RESTful API design**  
  (resources, verbs, status codes, uniform interface)
- See how **Minimal APIs** can replace controllers while remaining organized and testable
- Learn how to structure a project using **Domain → Application → Infrastructure → API** layers
- Integrate **MySQL** with **EF Core** in a clean, maintainable way
- Implement **pagination** and **filtering** for list endpoints
- Implement a **generic file upload** endpoint and update the corresponding user column
- Explain how the implementation satisfies the **REST constraints** during training
- Recognize how this design can evolve to include:
  - **JWT authentication & authorization**
  - Additional bounded contexts and entities

---

## Domain Model — Users

The demo uses a single domain entity: **User**.

### User fields

Table name: `users`

| Column            | Type             | Constraints                         | Notes                                   |
|-------------------|------------------|-------------------------------------|-----------------------------------------|
| `id`              | BIGINT           | PK, Auto Increment                  | Unique identifier                       |
| `user_name`       | VARCHAR(100)     | NOT NULL, UNIQUE                    | Used for login                          |
| `password_hash`   | VARCHAR(255)     | NOT NULL                            | Securely stored password hash           |
| `full_name`       | VARCHAR(200)     | NOT NULL                            | Display name                            |
| `role`            | ENUM             | NOT NULL (`Admin`, `read_only`)     | Access level / permissions              |
| `email_id`        | VARCHAR(255)     | NOT NULL                            | Email address                           |
| `mobile_num`      | VARCHAR(20)      | NOT NULL                            | Mobile number                           |
| `profile_pic_url` | VARCHAR(500)     | NULL                                | URL/path to profile picture file        |
| `created_at`      | DATETIME         | NOT NULL, default current timestamp | Row creation timestamp                  |

> **Important for training**:  
> - The input model exposes `password`, but the database stores a **hash** (`password_hash`).  
> - This is a good teaching moment to explain why plain-text passwords are unsafe.

---

## High-Level Architecture

The project follows a **layered / clean architecture** approach, typical for enterprise systems that may eventually grow to hundreds of tables and multiple bounded contexts.

```text
[ Client ]  (Postman, Browser, Web SPA, Mobile App)
        |
        v
+-------------------------------------------+
| Users.Api (Minimal APIs)                  |
| - HTTP endpoints (UsersEndpoints)         |
| - Routing, input/output models            |
| - Swagger / OpenAPI                       |
| - Basic landing page (/)                  |
+-------------------------------------------+
        |
        v
+-------------------------------------------+
| Users.Application                         |
| - Use cases / services (UserService)      |
| - DTOs (CreateUserRequest, UserDto, etc.) |
| - Contracts (IUserRepository)             |
+-------------------------------------------+
        |
        v
+-------------------------------------------+
| Users.Domain                              |
| - Entities (User)                         |
| - Enums (UserRole)                        |
| - Domain rules                            |
+-------------------------------------------+
        |
        v
+-------------------------------------------+
| Users.Infrastructure                      |
| - EF Core + MySQL                         |
| - DbContext, entity configurations        |
| - Migrations (InitialCreate)              |
| - Repository implementations              |
+-------------------------------------------+
        |
        v
[ MySQL Database ]
```

The **API layer is intentionally thin**.  
Business rules, mapping, and persistence concerns live in **Application**, **Domain**, and **Infrastructure**.

---

## Solution & Folder Structure

Actual layout under the repository root:

```text
restful_api_training/
├── LICENSE
├── README.md               # This file – architecture & code overview
├── RESTful.md              # Theory: RPC → SOAP → REST → etc.
├── restful_api_training.slnx
└── src
    └── Users
        ├── Users.Api/      # HTTP endpoints, Swagger, startup
        │   ├── appsettings.Development.json
        │   ├── appsettings.json
        │   ├── Endpoints/
        │   │   └── UsersEndpoints.cs      # Minimal API route mappings for /api/users
        │   ├── Program.cs                 # Composition root, DI, Swagger, OpenAPI
        │   ├── Properties/
        │   │   └── launchSettings.json
        │   ├── uploads/
        │   │   └── profilepic/            # Uploaded files (runtime, usually gitignored)
        │   ├── Users.Api.csproj
        │   └── Users.Api.http             # HTTP file for quick API testing (VS Code)
        │
        ├── Users.Application/             # Use cases, DTOs, interfaces
        │   ├── Contracts/
        │   │   └── IUserRepository.cs     # Abstraction for user persistence
        │   ├── DependencyInjection/
        │   │   └── ApplicationServiceCollectionExtensions.cs
        │   ├── Users/
        │   │   ├── Dtos/
        │   │   │   ├── CreateUserRequest.cs
        │   │   │   ├── UpdateUserRequest.cs
        │   │   │   ├── UserDto.cs
        │   │   │   └── UserFilterRequest.cs
        │   │   └── Services/
        │   │       └── UserService.cs     # Business logic around users
        │   └── Users.Application.csproj
        │
        ├── Users.Domain/                  # Core business model
        │   ├── Entities/
        │   │   └── User.cs
        │   ├── Enums/
        │   │   └── UserRole.cs
        │   └── Users.Domain.csproj
        │
        └── Users.Infrastructure/          # EF Core + MySQL, repositories, DI
            ├── DependencyInjection/
            │   └── InfrastructureServiceCollectionExtensions.cs
            ├── Persistence/
            │   ├── Configurations/
            │   │   └── UserConfiguration.cs
            │   ├── Migrations/
            │   │   ├── 20251208143050_InitialCreate.cs
            │   │   ├── 20251208143050_InitialCreate.Designer.cs
            │   │   └── UsersDbContextModelSnapshot.cs
            │   ├── UsersDbContext.cs
            │   └── UsersDbContextFactory.cs
            ├── Repositories/
            │   └── UserRepository.cs
            └── Users.Infrastructure.csproj
```

> During training, you can explicitly connect each folder to its responsibility, and show how this structure can be replicated for other bounded contexts (e.g., `Tickets`, `Orders`, `Billing`, etc.).

---

## Technology Stack

- **Runtime / Framework**
  - .NET 10
  - ASP.NET Core **Minimal APIs** (no MVC controllers)

- **Language**
  - C# (latest available with .NET 10)

- **Database**
  - MySQL 8+ (e.g., running in Docker)

- **Data Access**
  - Entity Framework Core
  - MySQL provider: **Pomelo.EntityFrameworkCore.MySql**

- **Tooling**
  - Swagger / OpenAPI for interactive API docs
  - `appsettings.json` + environment variables for configuration
  - Postman or HTTP file (`Users.Api.http`) for testing

- **Planned (not yet implemented)**
  - JWT Bearer Authentication
  - Role-based Authorization (`Admin`, `read_only`)
  - Dedicated auth endpoints

---

## REST API Design

Base route for the resource: **`/api/users`**

### Endpoint Summary

All routes are implemented using **Minimal APIs** with an endpoint group in `UsersEndpoints.cs`.

> **Note:** Authentication & authorization are **not enabled yet**.  
> For now, all endpoints are open for training/demo purposes.

| # | Operation                                     | Method | URL                                      |
|---|-----------------------------------------------|--------|------------------------------------------|
| 1 | Create user                                   | POST   | `/api/users`                             |
| 2 | Get user by ID                                | GET    | `/api/users/id/{id}`                     |
| 3 | Get all users (paged)                         | GET    | `/api/users?skip={x}&take={y}`           |
| 4 | Get all users (no paging)                     | GET    | `/api/users/all`                         |
| 5 | Filtered users (paged)                        | GET    | `/api/users/filter?...&skip=&take=`      |
| 6 | Filtered users (no paging)                    | GET    | `/api/users/filter/all?...`              |
| 7 | Update user                                   | PUT    | `/api/users/id/{id}`                     |
| 8 | Upload profile picture (generic)              | POST   | `/api/users/id/{id}/upload`              |
| 9 | Delete user                                   | DELETE | `/api/users/id/{id}`                     |

Additional non-API endpoints for convenience:

- `GET /` — HTML landing page explaining the project and linking to key URLs
- `GET /swagger` — Swagger UI
- `GET /openapi/v1.json` — OpenAPI JSON document
- `GET /weatherforecast` — Sample template endpoint (for reference)

### Notes on URL Design

- `GET /api/users/id/{id}` is intentionally used instead of `GET /api/users/{id}` to:
  - Avoid future ambiguity if alternate lookup routes are added (e.g., `/api/users/email/{email}`).
  - Create a clear pattern: `/api/users/{identifier-type}/{value}`.
- List endpoints follow standard REST practices with **query parameters** for pagination and filtering.

---

### Upload Endpoint

**URL**:  
`POST /api/users/id/{id}/upload`

**Request format**: `multipart/form-data`

Form-data fields:

| Field       | Type     | Required | Description                                          |
|------------|----------|----------|------------------------------------------------------|
| `file`     | binary   | Yes      | The file to upload                                   |
| `folder`   | string   | Yes      | Logical folder/group under storage (e.g. `profilepic`) |
| `fieldname`| string   | Yes      | Column/field name to update (e.g. `profile_pic_url`) |

**Behavior**:

1. The API stores the uploaded file into:

   ```text
   <content-root>/uploads/{folder}/<generated-file-name>
   ```

   For this project:

   ```text
   src/Users/Users.Api/uploads/profilepic/...
   ```

2. It computes a relative URL like:

   ```text
   /uploads/{folder}/{fileName}
   ```

3. It updates the **specified column** (e.g., `profile_pic_url`) in the `users` table for the given `id`.
4. It returns the updated `UserDto` JSON for convenience.

This endpoint is designed to be **generic** so it can support other file-related columns in future.

> For now, static file serving of `/uploads/...` from the API is a training discussion point and can be enabled with `app.UseStaticFiles()`.

---

### Error Handling & Status Codes

Common response patterns used in the project:

- `200 OK` — Successful GET / PUT / POST (non-creation) operations
- `201 Created` — Successful creation of a new user
- `204 No Content` — Successful deletion with no content body
- `400 Bad Request` — Validation errors, malformed input, missing data
- `404 Not Found` — User not found for given `id`
- `409 Conflict` — Duplicate username or conflicting constraints (where applicable)
- `500 Internal Server Error` — Unhandled errors (currently default behavior)

In a future iteration, you can add:

- A **global error handling middleware**
- A standard error response shape (e.g. `ProblemDetails`)

---

## DTOs & Request/Response Models

To separate **domain entities** from **API contracts**, the API uses DTOs and request models in the Application layer.

All of the below live under:  
`src/Users/Users.Application/Users/Dtos/`

### `UserDto` (response model)

```csharp
public sealed class UserDto
{
    public long Id { get; init; }
    public string UserName { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Role { get; init; } = default!; // "Admin" or "read_only"
    public string EmailId { get; init; } = default!;
    public string MobileNum { get; init; } = default!;
    public string? ProfilePicUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

### `CreateUserRequest`

```csharp
public sealed class CreateUserRequest
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Role { get; init; } = "read_only";  // default role
    public string EmailId { get; init; } = default!;
    public string MobileNum { get; init; } = default!;
}
```

### `UpdateUserRequest`

```csharp
public sealed class UpdateUserRequest
{
    public string? FullName { get; init; }
    public string? Role { get; init; }           // Optional change of role
    public string? EmailId { get; init; }
    public string? MobileNum { get; init; }
}
```

### `UserFilterRequest` (internal, used by service/repository)

```csharp
public sealed class UserFilterRequest
{
    public string? UserName { get; init; }
    public string? Role { get; init; }
    public string? EmailId { get; init; }
    public string? MobileNum { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }
}
```

The **Minimal APIs** (`UsersEndpoints.cs`) receive query/body parameters, construct these request objects, and pass them to `UserService`.

---

## Database Schema

Example conceptual SQL for the `users` table (EF Core migrations already generate this):

```sql
CREATE TABLE users (
    id               BIGINT PRIMARY KEY AUTO_INCREMENT,
    user_name        VARCHAR(100) NOT NULL UNIQUE,
    password_hash    VARCHAR(255) NOT NULL,
    full_name        VARCHAR(200) NOT NULL,
    role             ENUM('Admin', 'read_only') NOT NULL,
    email_id         VARCHAR(255) NOT NULL,
    mobile_num       VARCHAR(20) NOT NULL,
    profile_pic_url  VARCHAR(500) NULL,
    created_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### Seed Data (example)

Currently, an initial `admin` user was inserted directly via MySQL, e.g.:

```sql
INSERT INTO users (user_name, password_hash, full_name, role, email_id, mobile_num)
VALUES (
  'admin',
  '<sha256-hash-of-admin-password>',
  'API Admin',
  'Admin',
  'x@y.com',
  '9876543210'
);
```

You can use this user to test GET endpoints and uploads.

---

## REST Constraints & How This Project Demonstrates Them

1. **Client–Server**
   - The API does not render business UI; it exposes **JSON over HTTP**.
   - Clients (Postman, browser, future SPA, mobile app) are decoupled from server.

2. **Stateless**
   - No session state stored on the server.
   - Each request is self-contained.
   - Future JWT-based auth will further emphasise statelessness.

3. **Cacheable**
   - All `GET` endpoints are **safe** (do not modify server state).
   - They can be cached by clients or intermediaries (e.g., via headers in future).

4. **Uniform Interface**
   - Resource-oriented URLs: `/api/users`, `/api/users/id/{id}`, `/api/users/filter`.
   - Standard HTTP methods:
     - `GET` for read-only operations
     - `POST` for creation and upload
     - `PUT` for updates
     - `DELETE` for deletions
   - Standard HTTP status codes and JSON payloads.

5. **Layered System**
   - Clear separation between:
     - API (transport & HTTP concerns)
     - Application (use cases, orchestration)
     - Domain (core model)
     - Infrastructure (DB, EF Core, repositories)
   - The database, or even EF Core, could be swapped without changing API contracts.

6. **Code-on-Demand (optional)**
   - Not used in this project (typical for JSON APIs).
   - Good opportunity to explain that this is an **optional** REST constraint.

---

## Non-Functional Requirements (NFRs)

- **Statelessness**
  - No in-memory session; each request is independent.
- **Security**
  - Passwords stored as hashes (`password_hash`).
  - Authentication/authorization will be added later.
- **Correctness**
  - Explicit DTOs for create/update operations.
  - Clear HTTP status codes for common failure cases.
- **Performance**
  - Asynchronous APIs (`async/await`) and database calls.
  - Pagination and filtering supported for user listing.
- **Scalability**
  - Layered design allows scaling out API instances behind a load balancer.
  - Easy to extend domain model and add new bounded contexts by cloning the pattern.
- **Maintainability**
  - Clean separation of concerns using multiple projects.
  - Dependency Injection for swappable implementations.
- **Observability** (future)
  - Logging via built-in `ILogger`.
  - Later: structured logs, metrics, tracing, etc.

---

## Running the Project (Local Dev)

### 1. Prerequisites

- .NET 10 SDK installed  
- MySQL 8+ running (e.g., via Docker)  
- VS Code (optional but recommended)  
- Postman or any HTTP client (or just use `Users.Api.http`)

### 2. Clone the repository

```bash
git clone https://github.com/<your-username>/restful_api_training.git
cd restful_api_training
```

### 3. Set up MySQL

Create a database:

```sql
CREATE DATABASE restful_api_training;
```

Make a note of:

- Host: `localhost`
- Port: `3306`
- User: e.g. `root`
- Password: e.g. `pa55word`

### 4. Configure connection string

Update `src/Users/Users.Api/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=restful_api_training;User=root;Password=pa55word;TreatTinyAsBoolean=false"
  }
}
```

(Adjust user/password as per your local setup.)

### 5. Apply EF Core migrations

From the repo root:

```bash
dotnet ef database update   --project src/Users/Users.Infrastructure   --startup-project src/Users/Users.Api
```

This will:

- Create the `users` table (and any future tables)
- Apply the `InitialCreate` migration

### 6. Run the API

From the repo root:

```bash
dotnet run --project src/Users/Users.Api
```

You should see logs similar to:

```text
Now listening on: http://localhost:5117
Application started. Press Ctrl+C to shut down.
Hosting environment: Development
Content root path: .../src/Users/Users.Api
```

### 7. Explore the API

Open a browser:

- Landing page:  
  `http://localhost:5117/`

- Swagger UI:  
  `http://localhost:5117/swagger`

- OpenAPI JSON:  
  `http://localhost:5117/openapi/v1.json`

- Example API calls:
  - `GET http://localhost:5117/api/users/all`
  - `GET http://localhost:5117/api/users?id=...`
  - `POST http://localhost:5117/api/users` (create)
  - `PUT http://localhost:5117/api/users/id/{id}` (update)
  - `DELETE http://localhost:5117/api/users/id/{id}` (delete)
  - `POST http://localhost:5117/api/users/id/{id}/upload` (multipart/form-data)

---

## Recommended Training Flow

Suggested sequence for a **60–90 minute training/demonstration**:

1. **Introduce the domain**
   - Explain the `users` entity and its fields.
   - Show the `users` table in MySQL (admin row, etc.).

2. **Walk through the architecture diagram**
   - Explain the layered structure.
   - Clarify which project does what (Api, Application, Domain, Infrastructure).

3. **Show the folder structure in the repo**
   - Walk line-by-line through the structure in VS Code.
   - Connect each folder to a responsibility: endpoints, services, repository, DbContext.

4. **Open `Program.cs`**
   - Show how Minimal APIs are configured.
   - Explain DI wiring: `AddApplication()`, `AddInfrastructure()`.
   - Show Swagger/OpenAPI configuration.
   - Show the landing page and why it’s handy.

5. **Walk through `UsersEndpoints.cs`**
   - Show each endpoint:
     - `POST /api/users`
     - `GET /api/users`
     - `GET /api/users/all`
     - `GET /api/users/id/{id}`
     - `PUT /api/users/id/{id}`
     - `DELETE /api/users/id/{id}`
     - `POST /api/users/id/{id}/upload`
   - Map each to the corresponding HTTP method and REST operation.

6. **Open `UserService.cs` and `UserRepository.cs`**
   - Show the separation between:
     - API (transport)
     - Application (business logic)
     - Infrastructure (EF Core queries)
   - Show how filters & pagination are implemented.

7. **Demo live with Postman or Swagger**
   - Create a new user.
   - List users (paged & unpaged).
   - Get a user by ID.
   - Update a user.
   - Upload a profile picture and inspect:
     - File on disk under `uploads/profilepic`
     - URL stored in `profile_pic_url` column.
   - Delete a user.

8. **Tie back to REST**
   - Show how verbs + URLs + status codes map to REST constraints.
   - Discuss statelessness and how JWT auth will fit in later.

---

## Future Extensions

After this base training project, natural extensions include:

- **Authentication & Authorization**
  - Add `POST /api/auth/login`
  - Issue JWT tokens
  - Protect endpoints using roles (`Admin`, `read_only`)

- **Better Pagination**
  - Introduce a `PagedResult<T>` wrapper with `totalCount`, `skip`, `take`, `hasMore`.

- **Validation & Error Handling**
  - Add FluentValidation or similar for DTOs.
  - Implement centralized error handling middleware and consistent error responses.

- **More Resources**
  - Add additional entities (e.g., `roles`, `permissions`, `audit_logs`) following the same pattern.
  - Show how the structure scales to 50–100+ tables.

- **Observability & Ops**
  - Add structured logging, correlation IDs, health checks, etc.

- **Front-end Client**
  - Build a small SPA or simple web UI that consumes this API.

---

This `README.md` is meant to serve as both:

- A **specification** of the current implementation for the training demo  
- A **guide** that trainees and reviewers can read to understand the architecture, design decisions, and how to run the project locally.

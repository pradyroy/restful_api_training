
# RESTful API Training — .NET 10 + MySQL (Minimal APIs)

*A hands-on learning project using Minimal APIs, Entity Framework Core, and JWT Authentication & Authorization*

Repository name: **`restful_api_training`**

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Learning Goals](#learning-goals)
3. [Domain Model — Users](#domain-model--users)
4. [High-Level Architecture](#high-level-architecture)
5. [Solution & Folder Structure](#solution--folder-structure)
6. [Technology Stack](#technology-stack)
7. [Authentication & Authorization](#authentication--authorization)
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
- **JWT-based authentication** with role-based authorization
- A **clean, layered architecture** that can scale to enterprise systems with hundreds of tables

The goal is to keep the **functionality small** (single resource: `users`) while designing the **architecture like a real enterprise application**. This allows learners to see how a simple CRUD demo fits into a much larger, production-ready design.

The main resource in this training project is the **`users`** table with full CRUD and a generic **profile picture upload** endpoint.

---

## Learning Goals

By working through this project, learners should be able to:

- Understand the basics of **RESTful API design** (resources, verbs, status codes, uniform interface).
- See how **Minimal APIs** can replace controllers while still remaining organized and testable.
- Learn how to structure a project using **Domain–Application–Infrastructure–API** layers.
- Integrate **MySQL** with **EF Core** in a clean, maintainable way.
- Implement **JWT authentication** and **role-based authorization** (`admin` vs `read_only`).
- Implement **pagination** and **filtering** for list endpoints.
- Implement a **generic file upload** endpoint and update the corresponding user column.
- Explain how the implementation satisfies the **REST constraints** during training.

---

## Domain Model — Users

The demo uses a single domain entity: **User**.

### User fields

Table name: `users`

| Column           | Type             | Constraints                         | Notes                                   |
|------------------|------------------|-------------------------------------|-----------------------------------------|
| `id`             | BIGINT           | PK, Auto Increment                  | Unique identifier                       |
| `user_name`      | VARCHAR(100)     | NOT NULL, UNIQUE                    | Used for login                          |
| `password_hash`  | VARCHAR(255)     | NOT NULL                            | Securely stored password hash           |
| `full_name`      | VARCHAR(200)     | NOT NULL                            | Display name                            |
| `role`           | ENUM             | NOT NULL (`admin`, `read_only`)     | Access level / permissions              |
| `email_id`       | VARCHAR(255)     | NOT NULL                            | Email address                           |
| `mobile_num`     | VARCHAR(20)      | NOT NULL                            | Mobile number                           |
| `profile_pic_url`| VARCHAR(500)     | NULL                                | URL to profile picture                  |
| `created_at`     | DATETIME         | NOT NULL, default current timestamp | Row creation timestamp                  |

> **Important for training**: 
> - The input model may expose `password`, but the database should always store a **hash** (`password_hash`).
> - You can explicitly highlight why storing plain-text passwords is unsafe.

---

## High-Level Architecture

The project follows a **layered / clean architecture** approach, which is typical for enterprise systems that may eventually grow to hundreds of tables and multiple bounded contexts.

```text
[ Client ]  (Postman, Web SPA, Mobile App)
        |
        v
+-------------------------------------------+
| Users.Api (Minimal APIs)                  |
| - HTTP endpoints                          |
| - Routing, input/output models            |
| - Swagger / OpenAPI                       |
+-------------------------------------------+
        |
        v
+-------------------------------------------+
| Users.Application                         |
| - Use cases / services                    |
| - DTOs, commands, queries                 |
| - Validation                              |
| - Interfaces for repositories & tokens    |
+-------------------------------------------+
        |
        v
+-------------------------------------------+
| Users.Domain                              |
| - Entities (User)                         |
| - Value objects, enums (UserRole)         |
| - Domain rules                            |
+-------------------------------------------+
        |
        v
+-------------------------------------------+
| Users.Infrastructure                      |
| - EF Core + MySQL                         |
| - DbContext, entity configurations        |
| - Repository implementations              |
| - JWT token generation                    |
| - File storage utilities                  |
+-------------------------------------------+
        |
        v
[ MySQL Database ]
```

The **API layer is intentionally thin**. All business rules, validation and persistence logic live in the Application, Domain, and Infrastructure layers.

---

## Solution & Folder Structure

Suggested layout under the repository root:

```text
restful_api_training/
├─ README.md
├─ src/
│  └─ Users/
│     ├─ Users.Api/                 # HTTP endpoints, Swagger, startup
│     │  ├─ Program.cs
│     │  ├─ appsettings.json
│     │  ├─ Endpoints/
│     │  │   ├─ UsersEndpoints.cs   # Minimal API route mappings for /api/users
│     │  │   └─ AuthEndpoints.cs    # Login / token endpoints
│     │  ├─ Middleware/
│     │  │   └─ ErrorHandlingMiddleware.cs
│     │  └─ DependencyInjection/
│     │      └─ ApiServiceCollectionExtensions.cs
│     │
│     ├─ Users.Application/         # Use cases, DTOs, interfaces
│     │  ├─ Contracts/
│     │  │   ├─ IUserRepository.cs
│     │  │   └─ ITokenService.cs
│     │  ├─ Users/
│     │  │   ├─ Dtos/
│     │  │   │   ├─ UserDto.cs
│     │  │   │   ├─ CreateUserRequest.cs
│     │  │   │   ├─ UpdateUserRequest.cs
│     │  │   │   └─ UserFilterRequest.cs
│     │  │   ├─ Services/
│     │  │   │   └─ UserService.cs
│     │  ├─ Auth/
│     │  │   ├─ LoginRequest.cs
│     │  │   └─ AuthService.cs
│     │  └─ DependencyInjection/
│     │      └─ ApplicationServiceCollectionExtensions.cs
│     │
│     ├─ Users.Domain/              # Core business model
│     │  ├─ Entities/
│     │  │   └─ User.cs
│     │  ├─ Enums/
│     │  │   └─ UserRole.cs
│     │  └─ DomainExceptions/
│     │      └─ DomainException.cs
│     │
│     └─ Users.Infrastructure/      # EF, MySQL, JWT, file storage
│        ├─ Persistence/
│        │   ├─ UsersDbContext.cs
│        │   ├─ Configurations/
│        │   │   └─ UserConfiguration.cs
│        │   └─ Migrations/
│        ├─ Repositories/
│        │   └─ UserRepository.cs
│        ├─ Auth/
│        │   └─ JwtTokenService.cs
│        ├─ Files/
│        │   └─ FileStorageService.cs
│        └─ DependencyInjection/
│            └─ InfrastructureServiceCollectionExtensions.cs
│
└─ tests/
   ├─ Users.Api.Tests/
   ├─ Users.Application.Tests/
   ├─ Users.Domain.Tests/
   └─ Users.Infrastructure.Tests/
```

> During the training, you can show how this structure can be copied and adapted to other bounded contexts (e.g., `Billing`, `Tickets`, `Orders`, etc.) for large enterprises.

---

## Technology Stack

- **Runtime / Framework**
  - .NET 10
  - ASP.NET Core Minimal APIs (no MVC controllers)

- **Language**
  - C# (latest available with .NET 10)

- **Database**
  - MySQL 8+

- **Data Access**
  - Entity Framework Core with MySQL provider (e.g., Pomelo)

- **Security**
  - JWT Bearer Authentication
  - Role-based Authorization (`admin`, `read_only`)

- **Tooling**
  - Swagger / OpenAPI for interactive API docs
  - `appsettings.json` + environment variables for configuration

---

## Authentication & Authorization

### Roles

- `admin`  
  - Full CRUD on `users`
  - Can upload profile pictures

- `read_only`  
  - Can call **only read endpoints**
  - Cannot create, update, delete, or upload files

### JWT Flow (Conceptual)

1. Client calls `POST /api/auth/login` with `user_name` and `password`.
2. API validates credentials using `Users.Application.AuthService` and the `IUserRepository`.
3. On success, a JWT is generated using `ITokenService` (implemented by `JwtTokenService` in Infrastructure).
4. The token includes claims:
   - `sub`  → user ID
   - `name` → `user_name`
   - `role` → `admin` or `read_only`
5. Client includes `Authorization: Bearer <token>` header in subsequent requests.
6. Minimal APIs enforce authorization using:
   - `.RequireAuthorization()` for general access
   - `.RequireAuthorization("AdminOnly")` for admin-only operations

---

## REST API Design

Base route for the resource: **`/api/users`**

### Endpoint Summary

> All routes are implemented using **Minimal APIs** with endpoint groups.

| # | Operation                                     | Method | URL                                      | Auth            |
|---|-----------------------------------------------|--------|------------------------------------------|-----------------|
| 1 | Create user                                   | POST   | `/api/users`                             | Admin only      |
| 2 | Get user by ID                                | GET    | `/api/users/id/{id}`                     | Authenticated   |
| 3 | Get all users (paged)                         | GET    | `/api/users?skip={x}&take={y}`           | Authenticated   |
| 4 | Get all users (no paging)                     | GET    | `/api/users/all`                         | Authenticated   |
| 5 | Filtered users (paged)                        | GET    | `/api/users/filter?field=value&...` + paging params | Authenticated |
| 6 | Filtered users (no paging)                    | GET    | `/api/users/filter/all?field=value&...`  | Authenticated   |
| 7 | Update user                                   | PUT    | `/api/users/id/{id}`                     | Admin only      |
| 8 | Upload profile picture (generic)              | POST   | `/api/users/id/{id}/upload`              | Admin only      |
| 9 | Delete user                                   | DELETE | `/api/users/id/{id}`                     | Admin only      |

> In addition, there will be **auth endpoints** like `POST /api/auth/login` for token issuance.

### Notes on URL Design

- `GET /api/users/id/{id}` is intentionally used instead of `GET /api/users/{id}` to:
  - Avoid ambiguity if alternate lookup routes are later added (e.g., `/api/users/email/{email}`).
  - Create a clear URL pattern: `/api/users/{identifier-type}/{value}`.
- List endpoints follow standard REST practices with **query parameters** for pagination and filtering.

---

### Upload Endpoint

**URL**:  
`POST /api/users/id/{id}/upload`

**Auth**: Admin only

**Request format**: `multipart/form-data`

Form-data fields:

| Field      | Type     | Required | Description                                  |
|-----------|----------|----------|----------------------------------------------|
| `file`    | binary   | Yes      | The file to upload                           |
| `folder`  | string   | Yes      | Logical folder/group under storage           |
| `fieldname` | string | Yes      | Column/field name to update (e.g. `profile_pic_url`) |

**Behavior**:

1. The API stores the uploaded file into a configured storage location.
2. It computes or constructs the public/relative URL for the file.
3. It updates the **specified column** (e.g., `profile_pic_url`) in the `users` table for the given `id`.
4. It returns the updated `UserDto` or a minimal response containing the new file URL.

This endpoint is designed to be **generic** so that in future it can support other file-related columns too.

---

### Error Handling & Status Codes

Some common response patterns:

- `200 OK` — Successful GET / PUT / some POST operations
- `201 Created` — Successful creation of a new user
- `204 No Content` — Successful deletion with no body
- `400 Bad Request` — Validation errors, malformed input
- `401 Unauthorized` — Missing or invalid JWT token
- `403 Forbidden` — Valid token but insufficient role (e.g., `read_only` user calling admin-only endpoint)
- `404 Not Found` — User not found for given `id`
- `500 Internal Server Error` — Unhandled errors captured by global error middleware

Global error handling middleware will convert unexpected exceptions into a consistent JSON error shape, such as:

```json
{
  "traceId": "abc123",
  "message": "An unexpected error occurred."
}
```

---

## DTOs & Request/Response Models

To separate **domain entities** from **API contracts**, the API uses DTOs and request models.

### Example DTOs

**UserDto** (response model):

```csharp
public sealed class UserDto
{
    public long Id { get; init; }
    public string UserName { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Role { get; init; } = default!; // "admin" or "read_only"
    public string EmailId { get; init; } = default!;
    public string MobileNum { get; init; } = default!;
    public string? ProfilePicUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

**CreateUserRequest**:

```csharp
public sealed class CreateUserRequest
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Role { get; init; } = "read_only";  // default
    public string EmailId { get; init; } = default!;
    public string MobileNum { get; init; } = default!;
}
```

**UpdateUserRequest**:

```csharp
public sealed class UpdateUserRequest
{
    public string? FullName { get; init; }
    public string? Role { get; init; }           // Optional change of role
    public string? EmailId { get; init; }
    public string? MobileNum { get; init; }
}
```

**UserFilterRequest** (for internal usage, derived from query parameters):

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

### Auth Models

**LoginRequest**:

```csharp
public sealed class LoginRequest
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
}
```

**LoginResponse** (if you choose to model it): 

```csharp
public sealed class LoginResponse
{
    public string AccessToken { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
}
```

---

## Database Schema

Example SQL for creating the `users` table (simplified):

```sql
CREATE TABLE users (
    id               BIGINT PRIMARY KEY AUTO_INCREMENT,
    user_name        VARCHAR(100) NOT NULL UNIQUE,
    password_hash    VARCHAR(255) NOT NULL,
    full_name        VARCHAR(200) NOT NULL,
    role             ENUM('admin', 'read_only') NOT NULL,
    email_id         VARCHAR(255) NOT NULL,
    mobile_num       VARCHAR(20) NOT NULL,
    profile_pic_url  VARCHAR(500) NULL,
    created_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

> In the real implementation, this table will be created and managed via **EF Core migrations** rather than manual SQL, but the schema is useful to show conceptually during training.

---

## REST Constraints & How This Project Demonstrates Them

1. **Client–Server**
   - The API does not render HTML UI; it only exposes JSON over HTTP.
   - Clients (Postman, browser apps, mobile apps) are completely decoupled.

2. **Stateless**
   - No session state is stored on the server.
   - Every request includes its own JWT token for authentication.
   - All state is persisted in the MySQL database.

3. **Cacheable**
   - GET endpoints are safe and can be cached by clients or intermediaries.
   - Future enhancements can add `ETag`, `Cache-Control`, etc.

4. **Uniform Interface**
   - Resource-oriented URLs (`/api/users`, `/api/users/id/{id}`).
   - Standard HTTP methods:
     - `GET` for read-only operations
     - `POST` for creation
     - `PUT` for full updates
     - `DELETE` for deletions
   - Standard HTTP status codes.
   - Request and response representations are JSON.

5. **Layered System**
   - Clear separation between API, Application, Domain, and Infrastructure.
   - Infrastructure can be replaced (e.g., MySQL → PostgreSQL) without changing the API contracts.

6. **Code-on-Demand (optional)**
   - Not used in this project (typical for JSON APIs).
   - You can still explain that it is an optional REST constraint.

---

## Non-Functional Requirements (NFRs)

- **Statelessness**: No in-memory session; all auth via JWT.
- **Security**:
  - Passwords stored only as secure hashes.
  - Role-based permissions enforced consistently.
- **Correctness**:
  - DTO validation for create/update operations.
  - Clear error messages and status codes.
- **Performance**:
  - Asynchronous APIs and database calls.
  - Efficient pagination and filtering for large lists.
- **Scalability**:
  - Layered design allows scaling horizontally (multiple API instances).
  - Easy to extend domain model and add new bounded contexts.
- **Maintainability**:
  - Clean separation of concerns.
  - Dependency Injection for swappable implementations.
  - Unit test projects per layer (API, Application, Domain, Infrastructure).
- **Observability** (optional to implement in training):
  - Structured logging via `ILogger`.
  - Potential integration with tracing/metrics later.

---

## Running the Project (Local Dev)

> These steps are indicative. Adjust as needed once you implement the code.

1. **Clone the repository**

```bash
git clone https://github.com/<your-username>/restful_api_training.git
cd restful_api_training
```

2. **Set up MySQL**

- Create a database, e.g. `restful_api_training`.
- Note down connection details (host, port, user, password).

3. **Configure connection string**

Update `src/Users/Users.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=restful_api_training;User=root;Password=your_password;"
  },
  "Jwt": {
    "Issuer": "restful_api_training",
    "Audience": "restful_api_clients",
    "SigningKey": "replace-with-a-very-long-secret-key"
  }
}
```

4. **Apply migrations**

From the `Users.Infrastructure` project (once created):

```bash
dotnet ef database update --project src/Users/Users.Infrastructure --startup-project src/Users/Users.Api
```

5. **Run the API**

From the `Users.Api` project:

```bash
dotnet run --project src/Users/Users.Api
```

6. **Explore with Swagger**

Open the browser at:

```
https://localhost:<port>/swagger
```

Use the Swagger UI to test the endpoints and inspect request/response models.

---

## Recommended Training Flow

Suggested sequence for a 60–90 minute training/demonstration:

1. **Introduce the domain**:
   - Explain the `users` entity and roles.

2. **Walk through the architecture diagram**:
   - Show the layered structure.
   - Highlight how Minimal APIs are only the outermost shell.

3. **Show the folder structure**:
   - Explain each project and its responsibilities.

4. **Look at the Minimal API setup** (`Program.cs`):
   - Show how endpoints are mapped (`MapGroup`, `MapGet`, `MapPost`, etc.).

5. **Explain JWT auth flow**:
   - Demo `POST /api/auth/login` and copy the token into Swagger's `Authorize` button.

6. **Demo endpoints** in this order:
   - `POST /api/users` (create, admin only)
   - `GET /api/users` (paged)
   - `GET /api/users/all` (no paging)
   - `GET /api/users/id/{id}` (single resource)
   - `PUT /api/users/id/{id}` (update)
   - `POST /api/users/id/{id}/upload` (file upload)
   - `DELETE /api/users/id/{id}` (delete)

7. **Tie back to REST constraints**:
   - Show how each constraint is satisfied in the implementation and URLs.

---

## Future Extensions

Potential extensions after the base training:

- Add more resources (e.g., `roles`, `permissions`, `audit_logs`).
- Implement soft deletes (`is_deleted` flag) for users.
- Implement advanced filtering with dynamic predicates.
- Add API versioning (e.g., `/api/v1/users`).
- Add caching (e.g., response caching or ETags for GET).
- Integrate with a front-end SPA (React/Angular) as a client.

---

This `README.md` is meant to serve as both:

- A **specification** for what the training project should implement.
- A **guide** that trainees and reviewers can read to understand the architecture and design decisions.

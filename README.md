# WorkspacesAndTasks.API

A RESTful Task Management API built with **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**. The system enables users to organize their workspaces, collaborate with team members, manage tasks, assign responsibilities, and track progress through a personal dashboard.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Database Design](#database-design)
- [Authentication & Authorization](#authentication--authorization)
- [API Endpoints](#api-endpoints)
- [Business Rules](#business-rules)
- [Configuration](#configuration)
- [Database Migrations](#database-migrations)
- [API Documentation](#api-documentation)
- [Error Handling](#error-handling)
- [Project Scope](#project-scope)

---

## Overview

WorkspacesAndTasks.API is a backend application designed to simplify project organization and task management for individuals and small teams.

Users can create workspaces, invite existing registered users as members, create and assign tasks, set priorities and due dates, and monitor task progress.

The API ensures that workspace data remains private and accessible only to authorized participants.

### Key Objectives

- Provide secure user authentication and authorization.
- Allow users to create and manage workspaces.
- Support collaboration through workspace membership.
- Enable task creation, assignment, and progress tracking.
- Provide personalized task listings and dashboard statistics.
- Maintain data integrity through Entity Framework Core and relational database constraints.

---

## Features

### 1. Authentication & Account Management

- User registration and login.
- Secure password hashing using ASP.NET Core Identity.
- JWT Bearer authentication.
- Protected API endpoints.
- Role-based access checks through workspace ownership and membership.

### 2. Workspace Management

- Create workspaces.
- Retrieve workspaces owned by or shared with the current user.
- View workspace details.
- Update workspace information (owner only).
- Delete workspaces along with their associated tasks and memberships.
- Paginated workspace listing.

### 3. Workspace Members

- Add existing registered users to a workspace using their email addresses.
- Retrieve workspace participants.
- Remove members (owner only).
- Prevent duplicate memberships.
- Automatically unassign tasks belonging to a removed member.

### 4. Task Management

- Create tasks within workspaces.
- Retrieve workspace tasks with pagination.
- Filter tasks by status and assignee.
- Retrieve individual task details.
- Update task information, status, priority, due date, and assignee.
- Delete tasks (workspace owner only).
- Assign tasks to workspace participants.
- Support unassigned tasks.

### 5. My Tasks

- Retrieve all tasks assigned to the authenticated user across different workspaces.
- Filter tasks by status.
- Support pagination.
- Ensure users can only retrieve their own assignments.

### 6. Dashboard

Provides a personalized overview containing:

- Workspaces owned by or shared with the current user.
- Total assigned tasks.
- Tasks To Do.
- Tasks In Progress.
- Completed tasks.

---

## Technologies

| Technology | Purpose |
|---|---|
| C# | Primary programming language |
| .NET 8 | Application framework |
| ASP.NET Core Web API | RESTful API development |
| Entity Framework Core | ORM and database access |
| SQL Server | Relational database |
| ASP.NET Core Identity | User and password management |
| JWT Bearer Authentication | API authentication |
| LINQ | Data querying and filtering |
| Data Annotations | Request validation |
| Swagger / OpenAPI | API documentation |
| Dependency Injection | Service registration and dependency management |

---

## Architecture

The solution follows a layered architecture inspired by Clean Architecture principles, separating API responsibilities, business contracts, and infrastructure concerns.

```text
WorkspacesAndTasks.API/
│
├── API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
│
├── Core/
│   ├── Entities/
│   ├── DTOs/
│   ├── Enums/
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Data/
│   ├── Configurations/
│   ├── Services/
│   └── Migrations/
│
├── .gitignore
├── README.md
└── WorkspacesAndTasks.sln
```

*The directory structure above represents the main architectural organization. Individual folders may contain additional subfolders.*

### Layer Responsibilities

**API Layer**
- Exposes HTTP endpoints through controllers.
- Handles HTTP requests and responses.
- Applies authentication and authorization.
- Configures middleware and dependency injection.

**Core Layer**
- Contains application entities and domain enums.
- Defines DTOs used for API communication.
- Defines service interfaces and application contracts.

**Infrastructure Layer**
- Implements application services.
- Contains Entity Framework Core DbContext.
- Manages database configurations and migrations.
- Handles persistence and data access.

### Design Principles

- Separation of Concerns.
- Dependency Injection.
- Interface-based service contracts.
- DTO-based data transfer.
- Centralized exception handling.
- Asynchronous database operations.
- No unnecessary architectural abstractions.

---

## Database Design

The application uses SQL Server with Entity Framework Core Code First migrations.

### Main Entities

| Entity | Description |
|---|---|
| ApplicationUser | Extends ASP.NET Core Identity user with additional user information. |
| Project | Represents a workspace owned by a registered user. |
| ProjectMember | Represents the relationship between users and workspaces. |
| TaskItem | Represents a task belonging to a workspace. |

### Entity Relationships

```text
ApplicationUser
    |
    |---- 1 : Many ---- Projects (Owner)
    |
    |---- Many : Many ---- Projects
    |                      (ProjectMembers)
    |
    |---- 1 : Many ---- Tasks (Assigned Tasks)


Project
    |
    |---- 1 : Many ---- ProjectMembers
    |
    |---- 1 : Many ---- Tasks


ProjectMember
    |
    |---- Many : 1 ---- ApplicationUser


TaskItem
    |
    |---- Many : 1 ---- Project
    |
    |---- Many : 0..1 ---- ApplicationUser (Assignee)
```

### Important Database Constraints

- `ProjectMembers` uses a composite primary key (`ProjectId`, `UserId`) to prevent duplicate memberships.
- A workspace owner is stored in `Projects.OwnerId` and is not duplicated in `ProjectMembers`.
- `Tasks.AssignedToUserId` is nullable to support unassigned tasks.
- Foreign keys enforce relationships between entities.
- Appropriate indexes improve query performance.
- Cascade delete is configured for workspace-related tasks and memberships.
- Removing a member unassigns their tasks instead of deleting them.

---

## Authentication & Authorization

The API uses ASP.NET Core Identity for user management and JWT Bearer tokens for authentication.

### Authentication Flow

1. A user registers using their account information.
2. ASP.NET Core Identity validates credentials and securely hashes passwords.
3. The user logs in with their email and password.
4. The API generates a signed JWT.
5. The client sends the token with protected requests.

**Authorization Header:**

```http
Authorization: Bearer <JWT_TOKEN>
```

### Authorization Rules

| Action | Workspace Owner | Workspace Member | Non-Participant |
|---|---|---|---|
| View workspace | Yes | Yes | No |
| Update workspace | Yes | No | No |
| Delete workspace | Yes | No | No |
| Add members | Yes | No | No |
| Remove members | Yes | No | No |
| View tasks | Yes | Yes | No |
| Create tasks | Yes | Yes | No |
| Update tasks | Yes | Yes | No |
| Delete tasks | Yes | No | No |
| View My Tasks | Yes | Yes | Own assignments only |
| View Dashboard | Yes | Yes | Own data only |

### Security Considerations

- Passwords are managed and hashed by ASP.NET Core Identity.
- JWT signing keys must be stored in configuration or user secrets, never hard-coded.
- Workspace and task access is validated against the authenticated user's identity.
- Non-participants receive `404 Not Found` when accessing private workspace resources.
- Members attempting owner-only operations receive `403 Forbidden`.

---

## API Endpoints

**Base URL:**

```text
/api
```

All endpoints require a valid JWT Bearer token unless explicitly marked as public.

### 2. Workspaces

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/projects` | Get owned and joined workspaces |
| POST | `/api/projects` | Create a workspace |
| GET | `/api/projects/{projectId}` | Get workspace details |
| PUT | `/api/projects/{projectId}` | Update workspace details |
| DELETE | `/api/projects/{projectId}` | Delete a workspace |

### 3. Workspace Members

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/projects/{projectId}/members` | Get workspace participants |
| POST | `/api/projects/{projectId}/members` | Add a member by email |
| DELETE | `/api/projects/{projectId}/members/{userId}` | Remove a member |

### 4. Tasks

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/projects/{projectId}/tasks` | Get workspace tasks |
| POST | `/api/projects/{projectId}/tasks` | Create a task |
| GET | `/api/tasks/{taskId}` | Get task details |
| PUT | `/api/tasks/{taskId}` | Update a task |
| DELETE | `/api/tasks/{taskId}` | Delete a task |
| GET | `/api/tasks/my` | Get tasks assigned to the current user |

### 5. Dashboard

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/dashboard` | Get the current user's dashboard |

### API Conventions

- Request and response bodies use JSON.
- Property names follow camelCase.
- Dates use ISO 8601 UTC format.
- Task statuses: `ToDo`, `InProgress`, `Completed`.
- Task priorities: `Low`, `Medium`, `High`.
- List endpoints support page-number pagination.
- Default page size: `10`.
- Maximum page size: `50`.

---

## Business Rules

The application enforces the following core business rules:

1. Only authenticated users can access protected resources.
2. The creator of a workspace automatically becomes its owner.
3. Workspace ownership cannot be transferred.
4. Only workspace owners can update or delete their workspaces.
5. Only workspace owners can add or remove members.
6. A user cannot be added to the same workspace more than once.
7. Workspace members can create, view, and update tasks.
8. Only workspace owners can delete tasks.
9. Tasks can only be assigned to the workspace owner or one of its members.
10. Tasks can remain unassigned.
11. Removing a member automatically unassigns their tasks in that workspace.
12. Deleting a workspace also deletes its associated tasks and memberships.
13. Users can only retrieve tasks assigned to their own accounts through My Tasks.
14. Dashboard task counts include only tasks assigned to the current user.

---

## Configuration

The application requires database connection and JWT configuration.

### Connection String

Example configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  }
}
```

### JWT Configuration

```json
{
  "Jwt": {
    "Key": "YOUR_SECURE_SECRET_KEY",
    "Issuer": "YOUR_ISSUER",
    "Audience": "YOUR_AUDIENCE"
  }
}
```

**Important:**

- Replace all placeholder values with your actual configuration.
- Never commit real database passwords or JWT signing keys.
- Use .NET User Secrets for local development.
- Use environment variables or a secure configuration provider in production.

Example User Secrets commands:

```bash
dotnet user-secrets init --project API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING" --project API

dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY" --project API
```

The configuration keys must match the ones used by the application.

---

## Database Migrations

This project uses the Entity Framework Core Code First approach.

To create a new migration:

```bash
dotnet ef migrations add MigrationName
```

To apply migrations:

```bash
dotnet ef database update
```

To generate a SQL script:

```bash
dotnet ef migrations script
```

If `DbContext` and the startup project are in separate projects, provide the appropriate project arguments.

---

## API Documentation

Swagger / OpenAPI is integrated to provide interactive API documentation.

After running the application, open the Swagger UI using the configured Swagger URL, typically:

```text
https://localhost:<PORT>/swagger
```

Swagger allows developers to:

- Explore available endpoints.
- Inspect request and response models.
- Test API operations.
- Authenticate using JWT Bearer tokens through the **Authorize** button.
- Review supported HTTP status codes.

---

## Error Handling

The API uses centralized exception handling and a consistent error response format.

Validation errors follow a structured response:

```json
{
  "status": 400,
  "title": "Validation failed",
  "detail": "One or more fields are invalid.",
  "errors": {
    "title": [
      "Title is required."
    ]
  }
}
```

### Common HTTP Status Codes

| Status Code | Description |
|---|---|
| 200 | OK – Request completed successfully |
| 201 | Created – Resource created successfully |
| 204 | No Content – Operation completed without a response body |
| 400 | Bad Request – Invalid request or validation failure |
| 401 | Unauthorized – Missing or invalid authentication token |
| 403 | Forbidden – Authenticated participant lacks the required permission |
| 404 | Not Found – Resource does not exist or is not accessible |
| 409 | Conflict – Duplicate resource or membership |
| 500 | Internal Server Error – Unexpected server-side error |

Unexpected exceptions should return a generic error response without exposing internal implementation details or stack traces.

---

## Project Scope

This release focuses on the essential functionality required for workspace collaboration and task management.

The following features are intentionally outside the scope of version 1:

- Task comments.
- Task activity history and audit logs.
- Notifications and real-time updates.
- File attachments.
- Subtasks and task dependencies.
- Recurring tasks.
- Calendar integrations.
- Advanced analytics and reporting.
- Chat and messaging.
- Payments and subscriptions.
- Microservices architecture.
- Complex workspace roles and ownership transfer.

---

## Author

**Ahmed Sultan**

.NET Backend Developer

[LinkedIn](https://www.linkedin.com/in/ahmed-sultan-260010374)

---

## License

This project is available for educational and portfolio purposes.

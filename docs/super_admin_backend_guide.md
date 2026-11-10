# Backend Implementation Guide: Super Admin Workflow

This document outlines the architecture, database design, and API structure for the .NET Core Web API backend, focusing strictly on the **Super Admin Management** workflow.

## 1. Architectural Approach
We will use a simplified **Clean Architecture** tailored for a 72-hour MVP. Over-engineering will cost time, so we will stick to three core projects within your `.sln`:
*   **`Core` (Domain):** Entities, Enums, and Repository Interfaces.
*   **`Infrastructure`:** EF Core DbContext, Migrations, Repositories, and JWT Token Generators.
*   **`API`:** Controllers, Program.cs (DI setup), Swagger config, and Dockerfile.

## 2. Database Design (PostgreSQL / Neon)
The Super Admin needs to manage hierarchies. Here is the explicit entity structure using Data Annotations as preferred.

### Core Entities
*   **`User`**: Base identity table.
    *   Fields: `Id`, `Email`, `PasswordHash`, `Role` (Enum: SuperAdmin, DeptAdmin, Lecturer, Student), `DepartmentId` (Nullable), `IsActive`, `CreatedAt`.
*   **`Faculty`**: 
    *   Fields: `Id`, `Name`, `Code`, `CreatedAt`.
*   **`Department`**:
    *   Fields: `Id`, `Name`, `Code`, `FacultyId`, `CreatedAt`.
*   **`AcademicSession`**:
    *   Fields: `Id`, `Name` (e.g., "2026/2027"), `StartDate`, `EndDate`, `IsActive`.
*   **`Semester`**:
    *   Fields: `Id`, `Name` (e.g., "First Semester"), `SessionId`, `IsActive`.
*   **`AuditLog`**: (Crucial for the 20% security/RBAC score)
    *   Fields: `Id`, `UserId`, `Action` (e.g., "Created Faculty"), `EntityName`, `EntityId`, `Timestamp`.

## 3. Security & Role-Based Access Control (RBAC)
Since this is an MVP, skip Identity Framework overhead if you are comfortable hand-rolling a secure JWT issuer. 
1.  **Authentication:** A `/api/auth/login` endpoint that accepts email/password and returns a JWT.
2.  **Authorization:** The JWT must include a `role` claim. 
3.  **Backend Enforcement:** Use standard .NET authorization policies.
    ```csharp
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireSuperAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SuperAdmin"));
    });
    ```
    Apply `[Authorize(Policy = "RequireSuperAdmin")]` to every controller except Auth.

## 4. Required API Endpoints (The Super Admin Scope)

### Authentication
*   `POST /api/auth/login` - Returns JWT.
*   *Note: Seed a default Super Admin account in EF Core's `OnModelCreating` so the evaluator can log in immediately.*

### Faculty & Department Management
*   `POST /api/faculties` - Create a faculty.
*   `GET /api/faculties` - List all faculties (include Departments).
*   `POST /api/departments` - Create a department (requires FacultyId).
*   `GET /api/departments` - List departments.

### Academic Session Management
*   `POST /api/sessions` - Create academic session.
*   `POST /api/sessions/{id}/semesters` - Add a semester.
*   `PUT /api/sessions/{id}/activate` - Set as current session.

### User Management
*   `POST /api/users` - Create user (Admin, Lecturer, or Student).
*   `GET /api/users` - List users (filterable by Role).
*   `PUT /api/users/{id}/status` - Activate/Deactivate/Suspend account.
*   `PUT /api/users/{id}/assign-department` - Assign user to department.

### Audit & Monitoring (Read-Only)
*   `GET /api/audits` - List system activities.

## 5. Audit Trail Implementation (Pro-Tip)
Do not manually write logs in every controller. Override `SaveChanges` or `SaveChangesAsync` in your EF Core `DbContext`. Check `ChangeTracker.Entries()` for added/modified/deleted entities, construct an `AuditLog` record dynamically based on the current user's ID (injected via `IHttpContextAccessor`), and save it. This is a massive flex for a senior role.

## 6. Docker & Render Configuration
Place this exact `Dockerfile` in the `backend` folder:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
# Copy csproj and restore
COPY *.sln .
COPY API/*.csproj ./API/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY Core/*.csproj ./Core/
RUN dotnet restore
# Copy everything else and build
COPY . .
WORKDIR /app/API
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "API.dll"]
```
*   **CORS:** Explicitly allow your Vercel/Firebase Flutter URL in the `Program.cs` CORS policy. Do not use `AllowAnyOrigin` in the final submission.
using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.DTOs;

// --- Auth DTOs ---
public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    UserRole Role
);

// --- Faculty DTOs ---
public record CreateFacultyRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Code,
    string? Email = null,
    string? OfficeLocation = null,
    int? EstablishedYear = null,
    Guid? DeanUserId = null,
    string? DeanName = null
);

public record FacultyResponse(
    Guid Id,
    string Name,
    string Code,
    string? Email,
    string? OfficeLocation,
    int EstablishedYear,
    string? DeanName,
    bool IsActive,
    int TotalDepartmentsCount,
    int TotalStudentsCount,
    int TotalLecturersCount,
    DateTime CreatedAt,
    List<DepartmentResponse> Departments
);

// --- Department DTOs ---
public record CreateDepartmentRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Code,
    [Required] Guid FacultyId,
    string? Email = null,
    string? OfficeLocation = null,
    Guid? HodUserId = null,
    string? HodName = null
);

public record DepartmentResponse(
    Guid Id,
    string Name,
    string Code,
    Guid FacultyId,
    string? FacultyName,
    string? Email,
    string? OfficeLocation,
    string? HodName,
    int TotalCoursesCount,
    int TotalStudentsCount,
    bool IsActive,
    DateTime CreatedAt
);

// --- Session & Semester DTOs ---
public record CreateSessionRequest(
    [Required, MaxLength(50)] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate
);

public record CreateSemesterRequest(
    [Required, MaxLength(50)] string Name,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    DateTime? RegistrationStartDate = null,
    DateTime? RegistrationEndDate = null,
    DateTime? ExamStartDate = null,
    DateTime? ExamEndDate = null
);

public record SessionResponse(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    List<SemesterResponse> Semesters
);

public record SemesterResponse(
    Guid Id,
    string Name,
    Guid SessionId,
    bool IsActive,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? RegistrationStartDate,
    DateTime? RegistrationEndDate,
    DateTime? ExamStartDate,
    DateTime? ExamEndDate
);

public record AcademicMilestoneResponse(
    Guid Id,
    Guid SessionId,
    string Title,
    string? Subtitle,
    DateTime TargetDate,
    string Category
);

public record CreateAcademicMilestoneRequest(
    [Required, MaxLength(150)] string Title,
    string? Subtitle,
    [Required] DateTime TargetDate,
    string Category = "General"
);

// --- User DTOs ---
public record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] UserRole Role,
    Guid? DepartmentId,
    string? FirstName = null,
    string? LastName = null,
    string? Title = null,
    string? PhoneNumber = null,
    // Student fields
    string? MatricNumber = null,
    int? AcademicLevel = null,
    decimal? CurrentCgpa = null,
    Guid? AdvisorId = null,
    // Lecturer fields
    string? StaffId = null,
    string? EmploymentType = null,
    string? OfficeLocation = null,
    string? OfficeHours = null
);

public record UpdateUserStatusRequest(
    [Required] bool IsActive
);

public record AssignDepartmentRequest(
    Guid? DepartmentId
);

public record UserResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? FullName,
    string? Title,
    string? PhoneNumber,
    UserRole Role,
    Guid? DepartmentId,
    string? DepartmentName,
    bool IsActive,
    DateTime CreatedAt,
    // Student specifics
    string? MatricNumber,
    int? AcademicLevel,
    decimal? CurrentCgpa,
    int? CreditsEarned,
    string? AdvisorName,
    DateTime? AdmissionDate,
    // Lecturer specifics
    string? StaffId,
    string? EmploymentType,
    string? OfficeLocation,
    string? OfficeHours,
    int? CoursesCount,
    int? StudentsMentoredCount,
    int? PublicationsCount
);

// --- Course DTOs ---
public record CreateCourseRequest(
    [Required, MaxLength(20)] string Code,
    [Required, MaxLength(150)] string Title,
    int Credits = 3,
    int Level = 300,
    [Required] Guid DepartmentId = default
);

public record CourseResponse(
    Guid Id,
    string Code,
    string Title,
    int Credits,
    int Level,
    Guid DepartmentId,
    string? DepartmentName,
    bool IsActive,
    DateTime CreatedAt
);

public record CourseAllocationResponse(
    Guid Id,
    Guid CourseId,
    string CourseCode,
    string CourseTitle,
    Guid LecturerId,
    string LecturerName,
    Guid SessionId,
    int StudentsEnrolledCount
);

// --- Audit & RBAC DTOs ---
public record AuditLogResponse(
    Guid Id,
    Guid? UserId,
    string Action,
    string EntityName,
    string EntityId,
    string? Details,
    DateTime Timestamp
);

public record PermissionResponse(
    Guid Id,
    string Code,
    string Name,
    string Module,
    string Description
);

public record RolePermissionsResponse(
    UserRole Role,
    string RoleName,
    List<PermissionResponse> Permissions
);

public record UpdateRolePermissionsRequest(
    [Required] List<string> PermissionCodes
);

public record UserEffectivePermissionsResponse(
    Guid UserId,
    string Email,
    UserRole Role,
    List<string> Permissions
);

// --- System Telemetry DTO ---
public record SystemMetricsResponse(
    double SystemHealthPercentage,
    int ActiveDbPool,
    int MaxDbPool,
    int AvgResponseTimeMs,
    int TotalActiveUsers,
    int ActiveSessionsCount,
    DateTime ServerTimeUtc
);

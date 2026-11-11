using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.DTOs;

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

public record CreateFacultyRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Code
);

public record FacultyResponse(
    Guid Id,
    string Name,
    string Code,
    DateTime CreatedAt,
    List<DepartmentResponse> Departments
);

public record CreateDepartmentRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Code,
    [Required] Guid FacultyId
);

public record DepartmentResponse(
    Guid Id,
    string Name,
    string Code,
    Guid FacultyId,
    string? FacultyName,
    DateTime CreatedAt
);

public record CreateSessionRequest(
    [Required, MaxLength(50)] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate
);

public record CreateSemesterRequest(
    [Required, MaxLength(50)] string Name
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
    bool IsActive
);

public record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] UserRole Role,
    Guid? DepartmentId
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
    UserRole Role,
    Guid? DepartmentId,
    string? DepartmentName,
    bool IsActive,
    DateTime CreatedAt
);

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



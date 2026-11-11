namespace Core.Constants;

public static class Permissions
{
    public static class Faculties
    {
        public const string View = "faculties.view";
        public const string Create = "faculties.create";
        public const string Delete = "faculties.delete";
    }

    public static class Departments
    {
        public const string View = "departments.view";
        public const string Create = "departments.create";
        public const string Delete = "departments.delete";
    }

    public static class AcademicSessions
    {
        public const string View = "sessions.view";
        public const string Create = "sessions.create";
        public const string Activate = "sessions.activate";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string ManageStatus = "users.status.manage";
        public const string AssignDepartment = "users.department.assign";
    }

    public static class AuditLogs
    {
        public const string View = "audits.view";
    }

    public static class Rbac
    {
        public const string Manage = "rbac.manage";
    }

    public static IReadOnlyList<(string Code, string Name, string Module, string Description)> All = new[]
    {
        (Faculties.View, "View Faculties", "Faculties", "Permission to view faculties and nested departments"),
        (Faculties.Create, "Create Faculty", "Faculties", "Permission to create a new university faculty"),
        (Faculties.Delete, "Delete Faculty", "Faculties", "Permission to remove an existing faculty"),

        (Departments.View, "View Departments", "Departments", "Permission to browse university departments"),
        (Departments.Create, "Create Department", "Departments", "Permission to create a new department under a faculty"),
        (Departments.Delete, "Delete Department", "Departments", "Permission to delete an existing department"),

        (AcademicSessions.View, "View Academic Sessions", "Academic Sessions", "Permission to view sessions and semesters"),
        (AcademicSessions.Create, "Create Academic Session", "Academic Sessions", "Permission to create sessions and semesters"),
        (AcademicSessions.Activate, "Activate Academic Session", "Academic Sessions", "Permission to switch the active academic session"),

        (Users.View, "View Users", "Users", "Permission to view user lists and profiles"),
        (Users.Create, "Create User", "Users", "Permission to register new admins, lecturers, or students"),
        (Users.ManageStatus, "Manage User Status", "Users", "Permission to activate, suspend, or deactivate user accounts"),
        (Users.AssignDepartment, "Assign User Department", "Users", "Permission to assign or reassign users to departments"),

        (AuditLogs.View, "View Audit Trail", "Audit Logs", "Permission to inspect system audit trail and activity history"),

        (Rbac.Manage, "Manage Roles & Permissions", "RBAC", "Permission to view and dynamically configure role permissions")
    };
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Student;

    public Guid? DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Profile Information ---
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
        ? Email
        : $"{FirstName} {LastName}".Trim();

    [MaxLength(20)]
    public string? Title { get; set; } // "Prof.", "Dr.", "Mr.", "Mrs."

    [MaxLength(25)]
    public string? PhoneNumber { get; set; }

    // --- Student-Specific Fields ---
    [MaxLength(50)]
    public string? MatricNumber { get; set; } // e.g. "NAR/24/1001"

    public int? AcademicLevel { get; set; } // 100, 200, 300, 400, 500

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? CurrentCgpa { get; set; }

    public int CreditsEarned { get; set; } = 0;

    public Guid? AdvisorId { get; set; }

    [ForeignKey(nameof(AdvisorId))]
    public User? Advisor { get; set; }

    public DateTime? AdmissionDate { get; set; }

    // --- Lecturer-Specific Fields ---
    [MaxLength(50)]
    public string? StaffId { get; set; } // e.g. "STF-0142"

    [MaxLength(50)]
    public string? EmploymentType { get; set; } = "Full-time";

    [MaxLength(100)]
    public string? OfficeLocation { get; set; }

    [MaxLength(150)]
    public string? OfficeHours { get; set; }

    public int StudentsMentoredCount { get; set; } = 0;

    public int PublicationsCount { get; set; } = 0;
}

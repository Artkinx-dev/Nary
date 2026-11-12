using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Department
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public Guid FacultyId { get; set; }

    [ForeignKey(nameof(FacultyId))]
    public Faculty? Faculty { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? OfficeLocation { get; set; }

    public Guid? HodUserId { get; set; }

    [ForeignKey(nameof(HodUserId))]
    public User? HeadOfDepartment { get; set; }

    [MaxLength(100)]
    public string? HodName { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

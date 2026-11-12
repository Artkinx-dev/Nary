using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Course
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty; // e.g. "CSC 312"

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty; // e.g. "Advanced Data Structures"

    public int Credits { get; set; } = 3;

    public int Level { get; set; } = 300;

    [Required]
    public Guid DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseAllocation> Allocations { get; set; } = new List<CourseAllocation>();
}


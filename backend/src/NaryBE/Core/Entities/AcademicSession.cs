using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class AcademicSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = false;

    public ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    public ICollection<AcademicMilestone> Milestones { get; set; } = new List<AcademicMilestone>();

    public ICollection<CourseAllocation> Allocations { get; set; } = new List<CourseAllocation>();
}

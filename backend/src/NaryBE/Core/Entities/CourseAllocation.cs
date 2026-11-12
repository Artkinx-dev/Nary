using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class CourseAllocation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    [Required]
    public Guid LecturerId { get; set; }

    [ForeignKey(nameof(LecturerId))]
    public User? Lecturer { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public AcademicSession? Session { get; set; }

    public int StudentsEnrolledCount { get; set; } = 0;
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class AcademicMilestone
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public AcademicSession? Session { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Subtitle { get; set; }

    public DateTime TargetDate { get; set; }

    [MaxLength(50)]
    public string Category { get; set; } = "General"; // "Registration", "Exams", "Senate", "Holiday"
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Faculty
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? OfficeLocation { get; set; }

    public int EstablishedYear { get; set; } = 1998;

    public Guid? DeanUserId { get; set; }

    [ForeignKey(nameof(DeanUserId))]
    public User? Dean { get; set; }

    [MaxLength(100)]
    public string? DeanName { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
}

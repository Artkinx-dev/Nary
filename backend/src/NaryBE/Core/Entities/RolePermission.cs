using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities;

public class RolePermission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public UserRole Role { get; set; }

    [Required]
    public Guid PermissionId { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


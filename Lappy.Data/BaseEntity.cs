using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lappy.Data;

public abstract class BaseEntity
{
    [Column("id"), Required, Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [Column("company_id"), Required, MaxLength(36)]
    public string CompanyId { get; set; } = string.Empty;

    [Column("created_by_id"), Required, MaxLength(36)]
    public string CreatedById { get; set; } = string.Empty;

    [Column("created_at"), Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_by_id"), Required, MaxLength(36)]
    public string UpdatedById { get; set; } = string.Empty;

    [Column("updated_at"), Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

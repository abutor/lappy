using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lappy.Data.Models;

public class SessionEntity
{
    [Column("id"), Key]
    public Guid Id { get; set; }

    [Column("user_id"), Required]
    public Guid UserId { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    [Column("session_hash"), MaxLength(128)]
    public string SessionHash { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual AppUser User { get; set; }
}

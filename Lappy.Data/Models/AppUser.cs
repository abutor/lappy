using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Lappy.Data.Models;

[Table("users")]
public class AppUser
{
    [Key, Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(128), Column("email"), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Column("is_confirmed")]
    public bool IsConfirmed { get; set; }

    [MaxLength(128), Column("username")]
    public string UserName { get; set; } = string.Empty;

    [JsonIgnore, MaxLength(64), Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("description"), MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual CompanyEntity Company { get; set; }

    public virtual ICollection<SessionEntity> Sessions { get; set; } = [];
}

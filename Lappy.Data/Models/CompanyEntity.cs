using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lappy.Data.Models;

[Table("companies")]
public class CompanyEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("name"), Required, MaxLength(64)]
    public string Name { get; set; } = string.Empty;
}

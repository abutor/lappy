namespace Lappy.Core.Models;

public abstract class BaseModel
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsArchived {  get; set; }
}

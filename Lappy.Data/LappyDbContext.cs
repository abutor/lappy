using Lappy.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Lappy.Data;

public abstract class LappyDbContext : DbContext
{
    public virtual DbSet<AppUser> Users { get; set; }
    public virtual DbSet<SessionEntity> Sessions { get; set; }
}

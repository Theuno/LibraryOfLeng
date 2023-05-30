using Leng.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Leng.Infrastructure {
    public interface ILengDbContext {
        DbSet<MTGSets> MTGSets { get; set; }
        DbSet<MTGCards> MTGCard { get; set; }
        DbSet<LengUser> LengUser { get; set; }
        DbSet<LengUserMTGCards> LengUserMTGCards { get; set; }

        int SaveChanges();
    }
}

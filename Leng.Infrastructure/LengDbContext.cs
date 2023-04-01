using Leng.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Leng.Infrastructure {
    public class LengDbContext : DbContext {
        public LengDbContext(DbContextOptions<LengDbContext> options) : base(options) { }

        public LengDbContext() : this(new DbContextOptions<LengDbContext>()) { }

        public DbSet<MTGSets> MTGSets { get; set; }
        public DbSet<MTGCards> MTGCard { get; set; }
        public DbSet<LengUser> LengUser { get; set; }
        public DbSet<LengUserMTGCards> LengUserMTGCards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
        }
    }
}
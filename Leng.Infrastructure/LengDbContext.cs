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

            modelBuilder.Entity<LengUserMTGCards>()
                .HasOne(lumc => lumc.MTGCards)
                .WithMany()
                .HasForeignKey(lumc => lumc.MTGCardsID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LengUserMTGCards>()
                .HasOne(lumc => lumc.LengUser)
                .WithMany()
                .HasForeignKey(lumc => lumc.LengUserID)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<LengUserMTGCards>()
            //    .HasKey(x => new { x.LengUserID, x.MTGCardsID });

            //modelBuilder.Entity<LengUserMTGCards>()
            //    .HasOne(x => x.LengUser)
            //    .WithMany(x => x.LengUserMTGCards)
            //    .HasForeignKey(x => x.LengUserID);

            //modelBuilder.Entity<LengUserMTGCards>()
            //    .HasOne(x => x.MTGCards)
            //    .WithMany()
            //    .HasForeignKey(x => x.MTGCardsID);

            //modelBuilder.Entity<LengUserMTGCards>()
            //    .HasOne(lumc => lumc.MTGCards)
            //    .WithMany()
            //    .HasForeignKey(lumc => lumc.MTGCardsID);

            modelBuilder.Entity<LengUserDeck>()
                .HasKey(x => new { x.LengUserID, x.MTGDeckID });

            modelBuilder.Entity<LengUserDeck>()
                .HasOne(x => x.LengUser)
                .WithMany(x => x.LengUserDecks)
                .HasForeignKey(x => x.LengUserID);

            //modelBuilder.Entity<LengUserDeck>()
            //    .HasOne(x => x.MTGDeck)
            //    .WithMany(x => x.LengUserDecks)
            //    .HasForeignKey(x => x.MTGDeckID);

            //modelBuilder.Entity<MTGDeck>()
            //    .HasMany(d => d.Mainboard)
            //    .WithOne(c => c.MTGDeck)
            //    .HasForeignKey(c => c.MTGDeckID)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<MTGDeckCard>()
            //    .HasOne<MTGDeck>()
            //    .WithMany(deck => deck.Mainboard)
            //    .HasForeignKey(card => card.MTGDeckID);

            //modelBuilder.Entity<MTGDeckCard>()
            //    .HasOne<MTGCards>()
            //    .WithMany()
            //    .HasForeignKey(card => card.MTGCardsID);
        }
    }
}
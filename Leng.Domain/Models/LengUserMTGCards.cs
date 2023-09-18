using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leng.Domain.Models
{
    public class LengUserMTGCards
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        
        public string LengUserId { get; set; }  // Foreign key for LengUser
        [ForeignKey("LengUserId")]
        public LengUser LengUser { get; set; }

        public int MTGCardsId { get; set; }  // Foreign key for MTGCards
        [ForeignKey("MTGCardsId")]
        public MTGCards MTGCards { get; set; }

        public int count { get; set; }
        public int countFoil { get; set; }
        public int want { get; set; }
        public int wantFoil { get; set; }
    }

    [Index(nameof(LengUserMTGCards.LengUser), nameof(LengUserMTGCards.MTGCards), IsUnique = true)]
    public class LengUserMTGCardsConfiguration : IEntityTypeConfiguration<LengUserMTGCards>
    {
        public void Configure(EntityTypeBuilder<LengUserMTGCards> builder)
        {
            builder.HasIndex(e => new { e.LengUser, e.MTGCards }).IsUnique();
        }
    }
}

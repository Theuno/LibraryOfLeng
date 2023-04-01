using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leng.Domain.Models {
    public class LengUserMTGCards {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [ForeignKey("Id.LengUserID")]
        public LengUser LengUser { get; set; }

        [ForeignKey("Id.MTGCardsID")]
        public MTGCards MTGCards { get; set; }

        public int count { get; set; }
        public int countFoil { get; set; }
        public int want { get; set; }
        public int wantFoil { get; set; }
    }
}

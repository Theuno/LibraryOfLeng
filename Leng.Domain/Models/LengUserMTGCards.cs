using System.ComponentModel.DataAnnotations.Schema;

namespace Leng.Domain.Models {
    public class LengUserMTGCards {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [ForeignKey("LengUserID, LengUser")]
        public string LengUserID { get; set; }
        
        public LengUser LengUser { get; set; }

        [ForeignKey("MTGCards, MTGCardsID")]
        public int MTGCardsID { get; set; }
        public MTGCards MTGCards { get; set; }

        public int count { get; set; }
        public int countFoil { get; set; }
    }
}

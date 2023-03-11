using System.ComponentModel.DataAnnotations.Schema;

namespace Leng.Data.Models {
    public class LengUserMTGCards {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string LengUserID { get; set; }
        public LengUser LengUser { get; set; }

        public int MTGCardsID { get; set; }
        public MTGCards MTGCards { get; set; }

        public int count { get; set; }
    }
}

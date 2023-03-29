using Leng.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leng.Domain.Models {
    [PrimaryKey(nameof(LengUserID), nameof(MTGDeckID))]
    public class LengUserDeck {
        [ForeignKey("LengUser, LengUserID")]
        public string LengUserID { get; set; }
        public virtual LengUser LengUser { get; set; }

        [ForeignKey("MTGDeck, MGDeckID")]
        public virtual int MTGDeckID { get; set; }
        public virtual MTGDeck MTGDeck { get; set; }
    }
}

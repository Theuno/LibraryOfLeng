using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Leng.Domain.Models;

namespace Leng.Domain.Models {
    public class MTGDeck {
        public int MTGDeckID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int FormatID { get; set; }

        public string Format { get; set; }

        //public ICollection<MTGDeckCard> Mainboard { get; set; }

        //public ICollection<MTGDeckCard> Sideboard { get; set; }

        //public ICollection<LengUserDeck> LengUserDecks { get; set; }
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leng.Domain.Models
{
    public class LengUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string LengUserID { get; set; }
        public string? aduuid { get; set; }

        public ICollection<LengUserMTGCards> LengUserMTGCards { get; set; }
        public ICollection<LengUserDeck> LengUserDecks { get; set; }

    }
}

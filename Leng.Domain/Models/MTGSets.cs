using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using System.Text.Json.Serialization;
using static Leng.Domain.Models.MTGTranslations;

namespace Leng.Domain.Models
{
    [Index(nameof(setCode), IsUnique = true)]
    public class MTGSets
    {
        public int MTGSetsID { get; set; }
        public int baseSetSize { get; set; }
        public string? block { get; set; }
        //public string booster { get; set; }
        //cardsphereSetId INTEGER,

        [JsonPropertyName("code")]
        [Column(TypeName = "varchar(8)")]
        public string setCode { get; set; }

        public bool? isFoilOnly { get; set; }
        public bool? isForeignOnly { get; set; }
        public bool? isNonFoilOnly { get; set; }
        public Boolean isOnlineOnly { get; set; }
        public Boolean isPartialPreview { get; set; }
        //public string keyruneCode { get; set; }
        //public string languages { get; set; }
        public int? mcmId { get; set; }
        public int? mcmIdExtras { get; set; }
        public string? mcmName { get; set; }
        public string? mtgoCode { get; set; }
        public string? name { get; set; }
        public string? releaseDate { get; set; }

        //public string parentCode { get; set; }
        //public string sealedProduct { get; set; }
        //public int tcgplayerGroupId { get; set; }
        public string? tokenSetCode { get; set; }
        public int? totalSetSize { get; set; }
        public MTGTranslations? translations { get; set; }

        public string? type { get; set; }


        public List<MTGCards> Cards { get; set; }
    }
}

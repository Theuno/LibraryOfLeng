using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static System.Net.Mime.MediaTypeNames;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;

namespace Leng.Domain.Models
{ 
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [Index(nameof(name), nameof(setCode), nameof(number), IsUnique = true)]
    public class MTGCards : IComparable
    {
        public int CompareTo(object? obj)
        {
            MTGCards x1 = (MTGCards)obj;
            int x1Number = 0;
            int yNumber = 0;

            if(int.TryParse(x1.number, out x1Number) && int.TryParse(number, out yNumber))
            {
                return yNumber.CompareTo(x1Number);
            }
            else if (int.TryParse(x1.number, out x1Number))
            {
                // x1 is a number, y is a string
                var resultString = Regex.Match(number, @"\d+").Value;
                int.TryParse(resultString, out yNumber);
                return yNumber.CompareTo(x1Number);
            }
            else if (int.TryParse(number, out yNumber))
            {
                // y is a number, x1 is a string
                var resultString = Regex.Match(x1.number, @"\d+").Value;
                int.TryParse(resultString, out x1Number);
                return yNumber.CompareTo(x1Number);
            }
            else
            {
                // Both are strings
                var x1ResultString = Regex.Match(x1.number, @"\d+").Value;
                var yResultString = Regex.Match(number, @"\d+").Value;
                int.TryParse(x1ResultString, out x1Number);
                int.TryParse(yResultString, out yNumber);
                return yNumber.CompareTo(x1Number);
            }
        }

        private class SortCardsHelper : IComparer
        {
            int IComparer.Compare(object? x, object? y)
            {
                MTGCards x1 = (MTGCards)x;
                MTGCards y1 = (MTGCards)y;

                var regex = new Regex("^(d+)");

                var xRegexResult = regex.Match(x1.number);
                var yRegexResult = regex.Match(y1.number);

                // check if they are both numbers
                if (xRegexResult.Success && yRegexResult.Success)
                {
                    return int.Parse(xRegexResult.Groups[1].Value).CompareTo(yRegexResult.Groups[1].Value);
                }

                // otherwise return as string comparison
                return x1.number.CompareTo(y1.number);
            }
        }

        // Method to return IComparer object for sort helper.
        public static IComparer SortCardsAscending()
        {
            return new SortCardsHelper();
        }
        public MTGCards()
        {
            name = string.Empty; // or some other non-null value
        }

        public int MTGCardsID { get; set; }
        public string? artist { get; set; }

        // TODO: setCode should become a foreign key to MTGSets
        [JsonPropertyName("setCode")]
        [Column(TypeName = "varchar(8)")]
        public string? setCode { get; set; }
        /*
            [ForeignKey("setCode")]
            public virtual MTGSets MTGSets { get; set; }
        */

        public string name { get; set; }

        // Relationship to sets
        [Required]
        [ForeignKey("FK_MTGSets")]
        public int MTGSetsID { get; set; }

        [ForeignKey("MTGSetsID")]
        public MTGSets MTGSets { get; set; }

        public string? asciiName { get; set; }
        //attractionLights TEXT,
        //availability TEXT,
        //boosterTypes TEXT,
        //borderColor ENUM('black', 'white', 'borderless', 'silver', 'gold'),
        //cardKingdomEtchedId TEXT,
        //cardKingdomFoilId TEXT,
        //cardKingdomId TEXT,
        //cardParts TEXT,
        //cardsphereId TEXT,
        //colorIdentity TEXT,
        //colorIndicator TEXT,

        // This is a string that is not mapped to the source data.
        public string? color { get; set; }

        //convertedManaCost FLOAT,
        //duelDeck TEXT,
        public int edhrecRank { get; set; }
        public float edhrecSaltiness { get; set; }
        //faceConvertedManaCost FLOAT,
        //faceFlavorName TEXT,
        //faceManaValue FLOAT,

        public string? faceName { get; set; }

        //finishes TEXT,
        //flavorName TEXT,
        //flavorText TEXT,
        //frameEffects TEXT,
        //frameVersion ENUM('2003', '1993', '2015', '1997', 'future'),
        //hand TEXT,
        //hasAlternativeDeckLimit TINYINT(1) NOT NULL DEFAULT 0,
        //hasContentWarning TINYINT(1) NOT NULL DEFAULT 0,
        public bool? hasFoil { get; set; }
        public bool? hasNonFoil { get; set; }
        //isAlternative TINYINT(1) NOT NULL DEFAULT 0,
        //isFullArt TINYINT(1) NOT NULL DEFAULT 0,
        //isFunny TINYINT(1) NOT NULL DEFAULT 0,
        public bool isOnlineOnly { get; set; }
        //isOversized TINYINT(1) NOT NULL DEFAULT 0,
        //isPromo TINYINT(1) NOT NULL DEFAULT 0,
        //isRebalanced TINYINT(1) NOT NULL DEFAULT 0,
        //isReprint TINYINT(1) NOT NULL DEFAULT 0,
        //isReserved TINYINT(1) NOT NULL DEFAULT 0,
        //isStarter TINYINT(1) NOT NULL DEFAULT 0,
        //isStorySpotlight TINYINT(1) NOT NULL DEFAULT 0,
        //isTextless TINYINT(1) NOT NULL DEFAULT 0,
        //isTimeshifted TINYINT(1) NOT NULL DEFAULT 0,
        //keywords TEXT,
        //language TEXT,
        //leadershipSkills TEXT,
        //life TEXT,
        //loyalty TEXT,
        //manaCost TEXT,
        //manaValue FLOAT,
        public string? mcmId { get; set; }
        //mcmMetaId TEXT,
        //mtgArenaId TEXT,
        //mtgjsonFoilVersionId TEXT,
        //mtgjsonNonFoilVersionId TEXT,
        //mtgjsonV4Id TEXT,
        //mtgoFoilId TEXT,
        //mtgoId TEXT,
        //multiverseId TEXT,

        public string? number { get; set; }
        //originalPrintings TEXT,
        //originalReleaseDate TEXT,
        public string? originalText { get; set; }
        public string? originalType { get; set; }
        //otherFaceIds TEXT,
        public string? power { get; set; }
        //printings TEXT,
        //promoTypes TEXT,
        //purchaseUrls TEXT,
        //rarity ENUM('uncommon', 'common', 'rare', 'mythic', 'special', 'bonus'),
        //rebalancedPrintings TEXT,
        //relatedCards TEXT,
        public string? scryfallId { get; set; }
        //scryfallIllustrationId TEXT,
        //scryfallOracleId TEXT,
        //securityStamp TEXT,
        public string? side { get; set; }
        //signature TEXT,
        //subset TEXT,
        //subtypes TEXT,
        //supertypes TEXT,
        //tcgplayerEtchedProductId TEXT,
        //tcgplayerProductId TEXT,
        public string? text { get; set; }
        //toughness TEXT,
        public string? type { get; set; }
        //types TEXT,
        //uuid CHAR(36) UNIQUE NOT NULL,
        //variations TEXT,
        //watermark TEXT

        public ICollection<LengUserMTGCards>? LengUserMTGCards { get; set; }
    }
}

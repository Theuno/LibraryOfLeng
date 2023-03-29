using System.Text.Json.Serialization;

namespace Leng.Domain.Models
{
    public class MTGTranslations
    {
        public int MTGTranslationsID { get; set; }

        //[JsonPropertyName("Chinese Simplified")]
        //public object ChineseSimplified { get; set; }

        //[JsonPropertyName("Chinese Traditional")]
        //public object ChineseTraditional { get; set; }
        public string? French { get; set; }
        public string? German { get; set; }
        public string? Italian { get; set; }
        //public object Japanese { get; set; }
        //public object Korean { get; set; }

        //[JsonPropertyName("Portuguese (Brazil)")]
        //public object PortugueseBrazil { get; set; }
        //public object Russian { get; set; }
        public string? Spanish { get; set; }
    }
}
